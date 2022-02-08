using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundJobs
{
    public interface IBackgroundJobQueue
    {
        string Name { get; }
        ValueTask QueueBackgroundJobAsync(BackgroundJobQueueItem backgroundJobContainer);
        ValueTask<BackgroundJobQueueItem> DequeueAsync(CancellationToken cancellationToken);
        IAsyncEnumerable<BackgroundJobQueueItem> DequeueAllAsync(CancellationToken cancellationToken);
    }
}
