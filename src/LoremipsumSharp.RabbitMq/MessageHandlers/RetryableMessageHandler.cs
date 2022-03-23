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
    public class RetryableMessageHandler<T> : IMessageHandler<T> where T : IMessage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RetryConfiguration _retryConfiguration;
        private readonly RabbitSubscription _rabbitSubscriptionConfiguration;
        private readonly IMessageHandler<T> _handler;
        private readonly IConnection _conn;

        public RetryableMessageHandler(IServiceProvider serviceProvider,
                                       RetryConfiguration retryConfiguration,
                                       IMessageHandler<T> handler,
                                       RabbitSubscription rabbitSubscriptionConfiguration,
                                       IConnection connection)
        {
            _serviceProvider = serviceProvider;
            _retryConfiguration = retryConfiguration;
            _handler = handler;
            _rabbitSubscriptionConfiguration = rabbitSubscriptionConfiguration;
            _conn = connection;
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
                properties.ContentType = "UTF-8";
                properties.ContentType = "application/json";
                if (properties.Headers == null)
                    properties.Headers = new Dictionary<string, object>();
                var bodyBytes = basicDeliverEventArgs.Body.ToArray();
                var bodyString = Encoding.UTF8.GetString(bodyBytes);
                bodyBytes.TryDeserialize<T>(out var message);
                message.TryGetDescription(out var description);
                int retryTimes = 0;

                if (properties.Headers.TryGetValue("RetryTimes", out var retryTimesHeadVal))
                {
                    int.TryParse(Encoding.UTF8.GetString((byte[])retryTimesHeadVal), out retryTimes);
                }
                retryTimes++;
                if (retryTimes <= _retryConfiguration.MaxCount)
                {
                    var delay = _retryConfiguration.GetNextDelay(retryTimes);
                    properties.Headers.AddOrUpdate("RetryTimes", Encoding.UTF8.GetBytes(retryTimes.ToString()));
                    _ = Task.Delay(delay).ContinueWith((task, obj) =>
                     {
                         using var channel = _conn.CreateModel();
                         channel.BasicPublish(_rabbitSubscriptionConfiguration.Exchange, _rabbitSubscriptionConfiguration.RoutingKey, false, properties, bodyBytes);
                     }, default);
                }
                // 失败消息直接入库
                else
                {
                    var messageStorage = _serviceProvider.GetService<IMessageStorage>();
                    await messageStorage.StoreDeadLetter(bodyString, description, _rabbitSubscriptionConfiguration.AnonymousObjectToDictionary(x => x), ex);
                }
            }
        }
    }
}