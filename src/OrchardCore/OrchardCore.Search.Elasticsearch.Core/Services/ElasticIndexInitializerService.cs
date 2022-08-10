using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch
{
    /// <summary>
    /// Provides a way to initialize Elasticsearch index on startup of the application
    /// if the index is not found on the Elasticsearch server.
    /// </summary>
    public class ElasticIndexInitializerService : IModularTenantEvents
    {
        private readonly ShellSettings _shellSettings;

        public ElasticIndexInitializerService(ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public Task ActivatedAsync()
        {
            if (_shellSettings.State == TenantState.Running)
            {
                ShellScope.AddDeferredTask(async scope =>
                {
                    var elasticIndexSettingsService = scope.ServiceProvider.GetRequiredService<ElasticIndexSettingsService>();
                    var elasticIndexingService = scope.ServiceProvider.GetRequiredService<ElasticIndexingService>();
                    var indexManager = scope.ServiceProvider.GetRequiredService<ElasticIndexManager>();

                    var elasticIndexSettings = await elasticIndexSettingsService.GetSettingsAsync();

                    foreach (var settings in elasticIndexSettings)
                    {
                        if (!await indexManager.Exists(settings.IndexName))
                        {
                            await elasticIndexingService.CreateIndexAsync(settings);
                            await elasticIndexingService.ProcessContentItemsAsync(settings.IndexName);
                        }
                    }
                });
            }

            return Task.CompletedTask;
        }

        public Task ActivatingAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatedAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
