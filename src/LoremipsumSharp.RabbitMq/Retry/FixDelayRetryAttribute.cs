using System;


namespace LoremipsumSharp.RabbitMq.Retry
{
    public class FixDelayRetryAttribute : RetryConfiguration
    {
        public override TimeSpan GetNextDelay(int retryCount)
        {
            if (retryCount > this.MaxCount)
                return TimeSpan.Zero;
            return TimeSpan.FromMilliseconds(this.Interval);
        }
    }
}