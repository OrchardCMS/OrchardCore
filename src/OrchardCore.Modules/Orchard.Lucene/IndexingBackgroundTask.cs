using System;
using System.Threading;
using Orchard.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.Lucene
{
    /// <summary>
    /// This background task will index content items using.
    /// </summary>
    /// <remarks>
    /// This services is only registered from Orchard.Lucene.Worker feature.
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
