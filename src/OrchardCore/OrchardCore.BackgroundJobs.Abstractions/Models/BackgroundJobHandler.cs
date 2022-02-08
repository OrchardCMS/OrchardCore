using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundJobs.Models
{
    public abstract class BackgroundJobHandler<T> : IBackgroundJobHandler<T> where T : IBackgroundJob
    {
        public abstract ValueTask ExecuteAsync(T backgroundJob, CancellationToken cancellationToken);

        ValueTask IBackgroundJobHandler.ExecuteAsync(IBackgroundJob backgroundJob, CancellationToken cancellationToken)
            => ExecuteAsync((T)backgroundJob, cancellationToken);
    }
}
