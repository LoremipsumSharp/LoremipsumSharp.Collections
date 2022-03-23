using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq.Retry
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DeadLetterRetryAttribute :FixDelayRetryAttribute
    {

    }
}