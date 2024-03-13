using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Search.Lucene
{
    /// <summary>
    /// This background task will index content items using Lucene.
    /// </summary>
    /// <remarks>
    /// This services is only registered from OrchardCore.Search.Lucene.Worker feature.
    /// </remarks>
    [BackgroundTask(
        Title = "Lucene Indexes Updater",
        Schedule = "* * * * *",
        Description = "Updates lucene indexes.")]
    public class IndexingBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var indexingService = serviceProvider.GetService<LuceneIndexingService>();
            return indexingService.ProcessContentItemsAsync();
        }
    }
}
