using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundTasks
{
    public interface IBackgroundTaskEventHandler
    {
        Task ExecutingAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken);
        Task ExecutedAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken);
    }
}
