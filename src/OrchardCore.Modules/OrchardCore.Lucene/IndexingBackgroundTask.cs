using System;
using System.Threading;
using OrchardCore.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace OrchardCore.Lucene
{
    /// <summary>
    /// This background task will index content items using.
    /// </summary>
    /// <remarks>
    /// This services is only registered from OrchardCore.Lucene.Worker feature.
    /// </remarks>
    public class IndexingBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var indexingService = serviceProvider.GetService<LuceneIndexingService>();
            return indexingService.ProcessContentItemsAsync();
        }
    }
}
