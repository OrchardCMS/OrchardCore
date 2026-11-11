using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Jobs;
using OrchardCore.Jobs;
using OrchardCore.Modules;

namespace OrchardCore.Search.Indexing.Core;

/// <summary>
/// Keeps the content indexes up to date with the content items that changed. Each server updates the indexes it
/// keeps for itself, such as the ones on its disk. The indexes all the servers share, such as the ones of a remote
/// search service, are updated by <see cref="ContentIndexingJob"/>, that this task queues when they have changes to
/// index, so that a single server updates them.
/// </summary>
[BackgroundTask(
    Title = "Indexes Content Updater",
    Schedule = "* * * * *",
    Description = "Periodically synchronizes and updates all modified content items in the search indexes to ensure search results remain current.",
    LockTimeout = 1000,
    LockExpiration = 300000)]
public sealed class ContentIndexingBackgroundTask : IBackgroundTask
{
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var indexProfileStore = serviceProvider.GetRequiredService<IIndexProfileStore>();
        var indexingOptions = serviceProvider.GetRequiredService<IOptions<IndexingOptions>>().Value;
        var indexingService = serviceProvider.GetRequiredService<ContentIndexingService>();

        var localIndexes = await ContentIndexingJob.GetIndexProfilesAsync(indexProfileStore, indexingOptions, local: true);
        var sharedIndexes = await ContentIndexingJob.GetIndexProfilesAsync(indexProfileStore, indexingOptions, local: false);

        if (sharedIndexes.Count > 0)
        {
            await QueueSharedIndexesAsync(serviceProvider, indexingService, sharedIndexes.Select(indexProfile => indexProfile.Id).ToArray());
        }

        if (localIndexes.Count > 0)
        {
            await indexingService.ProcessRecordsAsync(localIndexes.Select(indexProfile => indexProfile.Id).ToArray());
        }
    }

    private static async Task QueueSharedIndexesAsync(IServiceProvider serviceProvider, ContentIndexingService indexingService, string[] indexIds)
    {
        try
        {
            // Nothing is queued while the indexes are up to date, so that an idle site doesn't write anything.
            if (!await indexingService.HasPendingTasksAsync(indexIds))
            {
                return;
            }

            // A run that didn't end yet is joined, and two runs are never executed at the same time.
            await serviceProvider.GetRequiredService<IJobManager>().EnqueueAsync<ContentIndexingJob>(configure: request =>
            {
                request.DeduplicationKey = ContentIndexingJob.Name;
                request.ConcurrencyKey = ContentIndexingJob.Name;
            });
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            // A search service that can't be reached must not prevent the indexes of this server from being updated.
            serviceProvider.GetRequiredService<ILogger<ContentIndexingBackgroundTask>>()
                .LogError(ex, "Error while checking the content indexes shared by the servers. It is tried again on the next run of the task.");
        }
    }
}
