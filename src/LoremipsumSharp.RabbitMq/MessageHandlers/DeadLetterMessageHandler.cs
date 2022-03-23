
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using LoremipsumSharp.RabbitMq.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LoremipsumSharp.Common;
using LoremipsumSharp.RabbitMq.Abstraction;
using LoremipsumSharp.RabbitMq.Extensions;

namespace LoremipsumSharp.RabbitMq.MessageHandlers
{
    public class DeadLetterRetryMessageHandler<T> : IMessageHandler<T> where T : IMessage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitSubscription _rabbitSubscriptionConfiguration;
        private readonly IMessageHandler<T> _handler;
        private readonly RetryConfiguration _retryConfiguration;
        private readonly IConnection _conn;
        private const string DeathCountHeaderName = "x-death";


        public DeadLetterRetryMessageHandler(IServiceProvider serviceProvider,
                                        RabbitSubscription rabbitSubscriptionConfiguration,
                                        IMessageHandler<T> handler,
                                        IConnection connection,
                                        RetryConfiguration retryConfiguration)
        {
            _serviceProvider = serviceProvider;
            _rabbitSubscriptionConfiguration = rabbitSubscriptionConfiguration;
            _handler = handler;
            _conn = connection;
            _retryConfiguration = retryConfiguration;
        }


        public async Task Handle(BasicDeliverEventArgs basicDeliverEventArgs)
        {

            try
            {
                await _handler.Handle(basicDeliverEventArgs);
            }
            catch (Exception ex)
            {
                var properties = basicDeliverEventArgs.BasicProperties;
                if (properties.Headers == null)
                {
                    properties.Headers = new Dictionary<string, object>();
                }


                var bodyBytes = basicDeliverEventArgs.Body.ToArray();
                var bodyString = Encoding.UTF8.GetString(bodyBytes);
                bodyBytes.TryDeserialize<T>(out var message);
                message.TryGetDescription(out var description);

                // retrive the x-death-count by header 
                var retryTimes = GetRetryTimes(properties.Headers);

                if (retryTimes <= _retryConfiguration.MaxCount)
                {
                    using var channel = _conn.CreateModel();
                    var delay = _retryConfiguration.GetNextDelay(retryTimes);
                    properties.Expiration = delay.TotalMilliseconds.ToString();
                    string deadLetterExchangeName = $"ErrorExchange_{_rabbitSubscriptionConfiguration.Queue}";
                    string deadLetterQueueName = $"{_rabbitSubscriptionConfiguration.Queue}.Deadletter";
                    string deadLetterRoutingKey = $"{_rabbitSubscriptionConfiguration.RoutingKey}.Deadletter";

                    channel.ExchangeDeclare(deadLetterExchangeName, "direct", true, true);
                    channel.QueueDeclare(deadLetterQueueName, true, false, false, new Dictionary<string, object>()
                    {
                        { "x-dead-letter-exchange", deadLetterExchangeName },
                        { "x-dead-letter-routing-key",_rabbitSubscriptionConfiguration.RoutingKey}
                    });

                    channel.QueueBind(_rabbitSubscriptionConfiguration.Queue, deadLetterExchangeName, _rabbitSubscriptionConfiguration.RoutingKey);
                    channel.QueueBind(deadLetterQueueName, deadLetterExchangeName, deadLetterRoutingKey);

                    channel.BasicPublish(deadLetterExchangeName, deadLetterRoutingKey, false, properties, bodyBytes);
                }
                else
                {
                    var messageStorage = _serviceProvider.GetService<IMessageStorage>();
                    await messageStorage.StoreDeadLetter(bodyString, description, _rabbitSubscriptionConfiguration.AnonymousObjectToDictionary(x => x), ex);

                }
            }
        }




        private static int GetRetryTimes(IDictionary<string, object> headers)
        {
            if (headers is null || !headers.ContainsKey(DeathCountHeaderName))
            {
                return 0;
            }

            var xDeath = (List<object>)headers[DeathCountHeaderName];

            if (xDeath is null || xDeath.Count == 0)
            {
                return 0;
            }

            var xDeathValues = (IDictionary<string, object>)xDeath.FirstOrDefault();

            if (xDeathValues is null || !xDeathValues.ContainsKey("count"))
            {
                return 0;
            }

            return int.Parse(xDeathValues["count"].ToString());
        }

    }
}