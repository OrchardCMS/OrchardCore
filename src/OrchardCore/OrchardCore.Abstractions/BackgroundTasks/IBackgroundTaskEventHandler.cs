using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskEventHandler
    {
        Task ExecutingAsync(BackgroundTaskContext context, CancellationToken cancellationToken);
        Task ExecutedAsync(BackgroundTaskContext context, CancellationToken cancellationToken);
    }
}
