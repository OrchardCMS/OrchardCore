using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

/// <summary>
/// This background task will index content items using Elasticsearch.
/// </summary>
/// <remarks>
/// This services is only registered from OrchardCore.Search.Elasticsearch.Worker feature.
/// </remarks>
[BackgroundTask(
    Title = "Elasticsearch Indexes Updater",
    Schedule = "* * * * *",
    Description = "Updates Elasticsearch indexes.",
    LockTimeout = 1000,
    LockExpiration = 300000)]
public sealed class IndexingBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var indexingService = serviceProvider.GetService<ElasticIndexingService>();

        if (indexingService != null)
        {
            return indexingService.ProcessContentItemsAsync();
        }

        return Task.CompletedTask;
    }
}
