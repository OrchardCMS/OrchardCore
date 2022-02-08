using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundJobs.Models
{
    public interface IBackgroundJobHandler
    {
        ValueTask ExecuteAsync(IBackgroundJob backgroundJob, CancellationToken cancellationToken);
    }

    public interface IBackgroundJobHandler<T> : IBackgroundJobHandler where T : IBackgroundJob
    {
        ValueTask ExecuteAsync(T backgroundJob, CancellationToken cancellationToken);
    }
}
