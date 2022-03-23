using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using Polly;
using Microsoft.Extensions.Logging;
using LoremipsumSharp.RabbitMq.Abstraction;

namespace LoremipsumSharp.RabbitMq
{
    public class RabbitMessageBus 
    {

        private readonly Func<string, IConnectionFactory> _connectionFactorySelector;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public RabbitMessageBus(Func<string, IConnectionFactory> connectionFactorySelector,
                                  IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            _connectionFactorySelector = connectionFactorySelector;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        public async Task<IMessageSubscription> Subscribe<T>(RabbitMqConsumeContext consumeContext, ILoggerFactory loggerFactory) where T : IMessage
        {

            var connectionFactory = _connectionFactorySelector(consumeContext.SubscriptionConfiguration.ConnectionKey);
            await Policy.Handle<Exception>()
            .WaitAndRetryAsync(int.MaxValue, attempt => TimeSpan.FromSeconds(10), (exception, duration, attempt, context) =>
             {
                 _logger.LogError($"unable to create the connection for subscription, attemt count:{attempt},exception:{exception.ToString()}");
             }).ExecuteAsync(() =>
             {
                 using var conn = connectionFactory.CreateConnection();
                 return Task.CompletedTask;
             });

            var subscription = new RabbitMqMessageSubscription<T>(consumeContext,connectionFactory,loggerFactory.CreateLogger<RabbitSubscription>());
            await subscription.StartAsync();
            return subscription;
        }

    }

}
