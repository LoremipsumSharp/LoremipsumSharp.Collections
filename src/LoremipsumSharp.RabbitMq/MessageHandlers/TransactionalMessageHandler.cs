using System;
using System.Threading.Tasks;
using System.Transactions;
using LoremipsumSharp.RabbitMq.Abstraction;
using RabbitMQ.Client.Events;

namespace LoremipsumSharp.RabbitMq
{
    public class TransactionalMessageHandler<T> : IMessageHandler<T> where T : IMessage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageHandler<T> _handler;

        public TransactionalMessageHandler(IMessageHandler<T> handler)
        {
            _handler = handler;
        }

        public async Task Handle(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.RepeatableRead,
            }, TransactionScopeAsyncFlowOption.Enabled);
            await _handler.Handle(basicDeliverEventArgs);
            ts.Complete();
        }
    }
}