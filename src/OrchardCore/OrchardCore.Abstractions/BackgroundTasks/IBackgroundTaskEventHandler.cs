using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    /// <summary>
    /// Provides methods that get called around the execution of any <see cref="IBackgroundTask"/>.
    /// Implement this interface to hook into background task execution.
    /// </summary>
    public interface IBackgroundTaskEventHandler
    {
        /// <summary>
        /// This method get called just before the execution of any <see cref="IBackgroundTask"/>.
        /// </summary>
        Task ExecutingAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken);

        /// <summary>
        /// This method get called just after the execution of any <see cref="IBackgroundTask"/>.
        /// </summary>
        Task ExecutedAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken);
    }
}
