using System;

namespace LoremipsumSharp.RabbitMq.Retry
{
    public interface IRetryStrategy
    {
        TimeSpan GetNextDelay(int retryCount);
    }
}