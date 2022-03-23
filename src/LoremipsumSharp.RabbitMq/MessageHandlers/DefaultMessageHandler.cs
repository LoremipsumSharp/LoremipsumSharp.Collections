using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoremipsumSharp.Common;
using LoremipsumSharp.RabbitMq.Abstraction;
using RabbitMQ.Client.Events;

namespace LoremipsumSharp.RabbitMq
{
    public class DefaultMessageHandler<T> : IMessageHandler<T> where T : IMessage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<object, Task> _handler;

        public DefaultMessageHandler(IServiceProvider serviceProvider, Func<object, Task> handler)
        {
            _serviceProvider = serviceProvider;
            _handler = handler;
        }


        public async Task Handle(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var bodyBytes = basicDeliverEventArgs.Body.ToArray();
            bodyBytes.TryDeserialize<T>(out var message);
            await _handler(message);
        }
    }
}