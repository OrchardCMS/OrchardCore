using System;
using System.Threading;
using Orchard.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.Lucene
{
    public class IndexingBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var indexingService = serviceProvider.GetService<LuceneIndexingService>();
            return indexingService.ProcessContentItemsAsync();
        }
    }
}
