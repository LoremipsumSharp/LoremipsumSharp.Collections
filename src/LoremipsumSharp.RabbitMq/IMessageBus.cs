
using LoremipsumSharp.RabbitMq.Abstraction;
using LoremipsumSharp.RabbitMq.Retry;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq
{
    public interface IMessageBus
    {
        Task<IMessageSubscription> Subscribe<T>(MethodInfo handleDefinition, Type handlerDeclareType, RabbitSubscription subscriptionConfiguration, RetryConfiguration retryConfiguration) where T : IMessage;
    }

}
