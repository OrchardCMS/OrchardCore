using System;
using System.Threading;

namespace Orchard.BackgroundTasks
{
    /// <summary>
    /// An implementation is instanciated once per host, and reused. <see cref="DoWork(IServiceProvider, CancellationToken)"/>
    /// is invoked periodically.
    /// </summary>
    public interface IBackgroundTask
    {
        void DoWork(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
