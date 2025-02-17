using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch;

/// <summary>
/// Provides a way to initialize Elasticsearch index on startup of the application
/// if the index is not found on the Elasticsearch server.
/// </summary>
public sealed class ElasticsearchIndexInitializerService : ModularTenantEvents
{
    private readonly ShellSettings _shellSettings;
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public ElasticsearchIndexInitializerService(
        ShellSettings shellSettings,
        ElasticsearchIndexManager elasticIndexManager,
        ElasticsearchIndexSettingsService elasticIndexSettingsService,
        IStringLocalizer<ElasticsearchIndexInitializerService> stringLocalizer,
        ILogger<ElasticsearchIndexInitializerService> logger)
    {
        _shellSettings = shellSettings;
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexSettingsService = elasticIndexSettingsService;
        S = stringLocalizer;
        _logger = logger;
    }

    public override async Task ActivatedAsync()
    {
        if (!_shellSettings.IsRunning())
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("elastic-initialize", async scope =>
        {
            var elasticIndexSettingsService = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexSettingsService>();
            var elasticIndexingService = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexingService>();
            var indexManager = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexManager>();

            var elasticIndexSettings = await elasticIndexSettingsService.GetSettingsAsync();
            var createdIndexes = new List<string>();

            foreach (var settings in elasticIndexSettings)
            {
                if (!await indexManager.ExistsAsync(settings.IndexName))
                {
                    await elasticIndexingService.CreateIndexAsync(settings);
                    createdIndexes.Add(settings.IndexName);
                }
            }

            if (createdIndexes.Count > 0)
            {
                await elasticIndexingService.ProcessContentItemsAsync(createdIndexes.ToArray());
            }
        });
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
