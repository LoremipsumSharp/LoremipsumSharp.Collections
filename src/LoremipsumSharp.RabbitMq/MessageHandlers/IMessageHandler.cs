using System.Threading.Tasks;
using LoremipsumSharp.RabbitMq.Abstraction;
using RabbitMQ.Client.Events;

namespace LoremipsumSharp.RabbitMq
{

    public interface IMessageHandler<T>  where T : IMessage
    {
        Task Handle(BasicDeliverEventArgs basicDeliverEventArgs);
    }
}