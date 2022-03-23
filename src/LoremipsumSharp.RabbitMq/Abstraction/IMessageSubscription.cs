using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq.Abstraction
{
    public interface IMessageSubscription : IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}