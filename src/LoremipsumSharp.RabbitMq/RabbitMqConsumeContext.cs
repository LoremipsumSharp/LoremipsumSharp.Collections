using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using LoremipsumSharp.RabbitMq.Retry;

namespace LoremipsumSharp.RabbitMq
{
    public class RabbitMqConsumeContext
    {
        public MethodInfo HandleDefinition { get; set; }
        public Type HandlerDeclareType { get; set; }
        public RabbitSubscription SubscriptionConfiguration { get; set; }
        public RetryConfiguration RetryConfiguration { get; set; }
        public IServiceScopeFactory ServiceScopeFactory { get; set; }
    }
}