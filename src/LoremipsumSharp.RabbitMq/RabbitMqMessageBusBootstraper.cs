
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using LoremipsumSharp.RabbitMq.Retry;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LoremipsumSharp.RabbitMq.Abstraction;

namespace LoremipsumSharp.RabbitMq
{

    public class RabbitMqMessageBusBootstraper : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly RabbitMessageBus _messageBus;
        private readonly RabbitMqMessagingOptions _rabbitMqMessagingOptions;

        public RabbitMqMessageBusBootstraper(IServiceScopeFactory serviceScopeFactory, RabbitMessageBus messageBus, IOptions<RabbitMqMessagingOptions> options, ILogger logger, ILoggerFactory loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _messageBus = messageBus;
            _rabbitMqMessagingOptions = options.Value;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            using var scope = _serviceScopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetService<IMessageStorage>();
            var lifeTime = scope.ServiceProvider.GetService<IHostApplicationLifetime>();
            await storage.InitSchema();

            var methods = AppDomain.CurrentDomain.GetAssemblies()
                 .Where(x => !x.GlobalAssemblyCache)
                 .SelectMany(x => x.GetTypes())
                 .Where(x => !_rabbitMqMessagingOptions.TypeFilters.Contains(x))
                 .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                 .Where(m => m.GetCustomAttributes<RabbitSubscription>().Any());

            foreach (var method in methods)
            {
                var subscriptionConfiguration = method.GetCustomAttribute<RabbitSubscription>();
                var retryConfiguration = method.GetCustomAttribute<RetryConfiguration>();
                var paramType = method.GetParameters().First().ParameterType;
                var genericMethod = typeof(RabbitMessageBus).GetMethod(nameof(RabbitMessageBus.Subscribe))
                    .MakeGenericMethod(paramType);
                var serviceType = method.DeclaringType;
                var consumeContext = new RabbitMqConsumeContext()
                {
                    HandleDefinition = method,
                    HandlerDeclareType = serviceType,
                    SubscriptionConfiguration = subscriptionConfiguration,
                    RetryConfiguration = retryConfiguration,
                    ServiceScopeFactory = _serviceScopeFactory,
                };
                var subscription = ((Task<IMessageSubscription>)genericMethod.Invoke(_messageBus, new object[] { consumeContext,_loggerFactory }))
                .ContinueWith((t, state) =>
                {
                    lifeTime.ApplicationStopped.Register(() =>
                    {
                        t.Result.Dispose();
                    });
                }, default);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
