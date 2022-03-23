using System;

namespace LoremipsumSharp.RabbitMq.Retry
{
    public class ExponentialBackoffRetryAttribute : RetryConfiguration
    {
        public override TimeSpan GetNextDelay(int retryCount)
        {
            if (retryCount > this.MaxCount)
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(Math.Pow(2, retryCount - 1) * this.Interval);
        }
    }
}