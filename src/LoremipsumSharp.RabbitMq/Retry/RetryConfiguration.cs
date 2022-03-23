using System;


namespace LoremipsumSharp.RabbitMq.Retry
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class RetryConfiguration : Attribute, IRetryStrategy
    {
        public ushort MaxCount { get; set; }
        public ushort Interval { get; set; }
        public abstract TimeSpan GetNextDelay(int retryCount);
    }
}