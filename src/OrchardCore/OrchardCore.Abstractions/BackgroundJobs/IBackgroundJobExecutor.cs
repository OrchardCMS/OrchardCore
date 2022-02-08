using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.BackgroundJobs
{
    /// <summary>
    /// Executes a background job within a shell context.
    /// </summary>
    public interface IBackgroundJobExecutor
    {
        ValueTask ExecuteAsync(IBackgroundJob backgroundJob, CancellationToken cancellationToken);
    }
}
