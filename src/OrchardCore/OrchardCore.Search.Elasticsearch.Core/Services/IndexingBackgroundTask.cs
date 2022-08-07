using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Search.Elasticsearch.Core.Services
{
    /// <summary>
    /// This background task will index content items using.
    /// </summary>
    /// <remarks>
    /// This services is only registered from OrchardCore.Search.Elasticsearch.Worker feature.
    /// </remarks>
    [BackgroundTask(Schedule = "* * * * *", Description = "Update Elasticsearch indexes.")]
    public class IndexingBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var indexingService = serviceProvider.GetService<ElasticIndexingService>();
            return indexingService.ProcessContentItemsAsync();
        }
    }
}
