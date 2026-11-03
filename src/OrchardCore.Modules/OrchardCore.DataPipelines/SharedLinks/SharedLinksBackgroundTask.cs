using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Deletes the shared links a day after they stopped working, and lets their handlers release their resources, such as
/// the copies of the shared files.
/// </summary>
[BackgroundTask(
    Title = "Shared links cleanup",
    Schedule = "*/15 * * * *",
    Description = "Deletes the shared links that stopped working, and their resources.",
    LockTimeout = 3_000,
    LockExpiration = 30_000)]
public sealed class SharedLinksBackgroundTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var manager = serviceProvider.GetRequiredService<SharedLinkManager>();
        var clock = serviceProvider.GetRequiredService<IClock>();

        foreach (var link in await manager.ListExpiredAsync(clock.UtcNow.AddDays(-1), 50, cancellationToken))
        {
            await manager.DeleteAsync(link, cancellationToken);
        }
    }
}
