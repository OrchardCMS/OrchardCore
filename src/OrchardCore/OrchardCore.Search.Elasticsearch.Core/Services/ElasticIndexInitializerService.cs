using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch
{
    /// <summary>
    /// Provides a way to initialize Elasticsearch index on startup of the application
    /// if the index is not found on the Elasticsearch server.
    /// </summary>
    public class ElasticIndexInitializerService : ModularTenantEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly ElasticIndexManager _elasticIndexManager;
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
        protected readonly IStringLocalizer S;
        private readonly ILogger _logger;

        public ElasticIndexInitializerService(
            ShellSettings shellSettings,
            ElasticIndexManager elasticIndexManager,
            ElasticIndexSettingsService elasticIndexSettingsService,
            IStringLocalizer<ElasticIndexInitializerService> localizer,
            ILogger<ElasticIndexInitializerService> logger)
        {
            _shellSettings = shellSettings;
            _elasticIndexManager = elasticIndexManager;
            _elasticIndexSettingsService = elasticIndexSettingsService;
            S = localizer;
            _logger = logger;
        }

        public override Task ActivatedAsync()
        {
            if (_shellSettings.IsRunning())
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

        public override async Task RemovingAsync(ShellRemovingContext context)
        {
            try
            {
                var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync();
                foreach (var settings in elasticIndexSettings)
                {
                    var result = await _elasticIndexManager.DeleteIndex(settings.IndexName);
                    if (!result)
                    {
                        _logger.LogError("Failed to remove the Elasticsearch index {IndexName}", settings.IndexName);
                        context.ErrorMessage = S["Failed to remove the Elasticsearch index '{0}'.", settings.IndexName];
                    }
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Failed to remove Elasticsearch indices");
                context.ErrorMessage = S["Failed to remove Elasticsearch indices."];
                context.Error = ex;
            }
        }
    }
}
