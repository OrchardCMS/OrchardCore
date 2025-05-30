using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Indexing.Core;

namespace OrchardCore.Search.Indexing.Core;

/// <summary>
/// This background task will index content items using all contents.
/// </summary>
[BackgroundTask(
    Title = "Indexes Content Updater",
    Schedule = "* * * * *",
    Description = "Periodically synchronizes and updates all modified content items in the search indexes to ensure search results remain current.",
    LockTimeout = 1000,
    LockExpiration = 300000)]
public sealed class IndexingBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var indexingService = serviceProvider.GetRequiredService<ContentIndexingService>();

        return indexingService.ProcessContentItemsForAllIndexesAsync();
    }
}
