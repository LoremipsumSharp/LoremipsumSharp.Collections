using Microsoft.Extensions.Configuration;
using LoremipsumSharp.RabbitMq;
using RabbitMQ.Client;
using System;
using System.Linq;
using LoremipsumSharp.RabbitMq.Abstraction;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQMessageBus(this IServiceCollection services, Action<RabbitMqMessagingOptions> setupAction)
        {
            var options = new RabbitMqMessagingOptions();
            setupAction.Invoke(options);
            services.Configure<RabbitMqMessagingOptions>(opt=>{
                setupAction?.Invoke(opt);
            });


            services.AddHostedService<RabbitMqMessageBusBootstraper>();
            services.AddSingleton<IChannelFactory, ChannelFactory>();
            services.AddSingleton<RabbitMessageBus>();
            var handlerTypes = options.AssembliesToScan
                                .SelectMany(x => x.GetTypes())
                                .Where(x => !options.TypeFilters.Contains(x))
                                    .Where(x => typeof(IMessageConsumer).IsAssignableFrom(x) && !x.IsAbstract).ToList();
            foreach (var handlerType in handlerTypes)
            {
                services.AddTransient(handlerType);
                services.AddTransient(typeof(IMessageConsumer), handlerType);
            }

            services.AddScoped<IMessageStorage, MySqlMessageStorage>(sp =>
            {
                return new MySqlMessageStorage(options.MySqlStorageConnectionString);
            });

            services.AddSingleton<Func<string, IConnectionFactory>>(sp =>
            {
                Func<string, IConnectionFactory> selector = key =>
                {
                    var connStr = options.ConfigurationSection.GetConnectionString(key);
                    if (string.IsNullOrEmpty(connStr)) throw new ArgumentNullException(nameof(connStr));
                    return new ConnectionFactory
                    {
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(30),
                        Uri = new Uri(connStr),
                        DispatchConsumersAsync = true,
                    };
                };
                return selector;
            });

            return services;

        }
    }
}

