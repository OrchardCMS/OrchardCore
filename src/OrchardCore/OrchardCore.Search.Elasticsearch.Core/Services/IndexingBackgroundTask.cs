using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Search.Elasticsearch.Core.Services
{
    /// <summary>
    /// This background task will index content items using Elasticsearch.
    /// </summary>
    /// <remarks>
    /// This services is only registered from OrchardCore.Search.Elasticsearch.Worker feature.
    /// </remarks>
    [BackgroundTask(Schedule = "* * * * *", Description = "Update Elasticsearch indexes.", LockTimeout = 1000, LockExpiration = 300000)]
    public class IndexingBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var indexingService = serviceProvider.GetService<ElasticIndexingService>();

            if (indexingService != null)
            {
                return indexingService.ProcessContentItemsAsync();
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
