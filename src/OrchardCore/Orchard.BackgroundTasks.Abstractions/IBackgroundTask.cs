using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orchard.BackgroundTasks
{
    /// <summary>
    /// An implementation is instanciated once per host, and reused. <see cref="DoWorkAsync(IServiceProvider, CancellationToken)"/>
    /// is invoked periodically.
    /// </summary>
    public interface IBackgroundTask
    {
        Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
