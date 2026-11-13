using OrchardCore.Jobs;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Deletes the shared links a day after they stopped working, and lets their handlers release their resources, such as
/// the copies of the shared files. It runs every 15 minutes, once whatever the number of servers.
/// </summary>
public sealed class SharedLinksCleanupJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "SharedLinksCleanup";

    private readonly SharedLinkManager _manager;
    private readonly IClock _clock;

    public SharedLinksCleanupJob(SharedLinkManager manager, IClock clock)
    {
        _manager = manager;
        _clock = clock;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var deleted = 0;

        foreach (var link in await _manager.ListExpiredAsync(_clock.UtcNow.AddDays(-1), 50, cancellationToken))
        {
            await _manager.DeleteAsync(link, cancellationToken);
            deleted++;
        }

        context.LogInformation($"{deleted} shared links were deleted.");
    }
}
