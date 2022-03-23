using LoremipsumSharp.RabbitMq.MessageHandlers;
using LoremipsumSharp.RabbitMq.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using LoremipsumSharp.RabbitMq.Abstraction;
using Microsoft.Extensions.Logging;

namespace LoremipsumSharp.RabbitMq
{
    public class RabbitMqMessageSubscription<T> : IMessageSubscription where T : IMessage
    {
        private readonly IConnection _connection;
        private readonly ILogger _logger ;
        private readonly IModel _channel;
        private AsyncEventingBasicConsumer _consumer;
        private RabbitMqConsumeContext _consumeContext;
        private IConnectionFactory connectionFactory;

        public RabbitMqMessageSubscription(RabbitMqConsumeContext consumeContext,
                                           IConnectionFactory connectionFactory, ILogger logger)
        {
            this.connectionFactory = connectionFactory;

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumer = new AsyncEventingBasicConsumer(_channel);

            _consumeContext = consumeContext;
            _logger = logger;
        }

        public void Dispose()
        {
            if (_channel.IsOpen)
            {
                if (_consumer.IsRunning)
                {
                    foreach (var tag in _consumer.ConsumerTags)
                    {
                        _channel.BasicCancel(tag);
                    }
                    _logger.LogError($"the consumer is canceld in dispose ,message source:{typeof(T).Name}");

                }
                _channel.Close();
            }
            _channel.Dispose();
            _logger.LogError($"message source is dispose ,name :{typeof(T).Name}");
            _connection.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            var subscriptionConfiguration = _consumeContext.SubscriptionConfiguration;
            var q = _channel.QueueDeclare(subscriptionConfiguration.Queue, true, false, false, null);
            _channel.QueueBind(subscriptionConfiguration.Queue, subscriptionConfiguration.Exchange, subscriptionConfiguration.RoutingKey, null);

        
            AsyncEventHandler<BasicDeliverEventArgs> onReceived =  async (sender, args) =>
            {
                var scope = _consumeContext.ServiceScopeFactory.CreateScope();
                Func<object, Task> rawHandler = async (object obj) =>
                {
                    var serviceImpl = scope.ServiceProvider.GetService(_consumeContext.HandlerDeclareType);
                    var t = (Task)_consumeContext.HandleDefinition.Invoke(serviceImpl, new object[] { obj });
                    await t;
                };
                IMessageHandler<T> handlerDecorator = new DefaultMessageHandler<T>(scope.ServiceProvider, rawHandler);
                if (_consumeContext.RetryConfiguration != null)
                {
                    if (_consumeContext.RetryConfiguration is DeadLetterRetryAttribute)
                        handlerDecorator = new DeadLetterRetryMessageHandler<T>(scope.ServiceProvider,
                                                                           _consumeContext.SubscriptionConfiguration,
                                                                           handlerDecorator,
                                                                           _connection,
                                                                           _consumeContext.RetryConfiguration);
                    else
                        handlerDecorator = new RetryableMessageHandler<T>(scope.ServiceProvider,
                                                                          _consumeContext.RetryConfiguration,
                                                                          handlerDecorator,
                                                                          _consumeContext.SubscriptionConfiguration,
                                                                          _connection);
                }
                try
                {
                      await handlerDecorator.Handle(args);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                finally
                {
                    _channel.BasicAck(args.DeliveryTag, false);
                    scope.Dispose();
                }
            };
            _consumer.Received += onReceived;
                _channel.BasicQos(0, (ushort)subscriptionConfiguration.PrefetchCount, false);
            _channel.BasicConsume(subscriptionConfiguration.Queue, false, _consumer);
            return Task.CompletedTask;
        }



        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}