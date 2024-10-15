using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Deployment.Models;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexSettingsStep : NamedRecipeStepHandler
{
    public const string Name = "azureai-index-create";

    private readonly AzureAISearchIndexManager _indexManager;
    private readonly AzureAIIndexDocumentManager _azureAIIndexDocumentManager;
    private readonly AzureAISearchIndexSettingsService _azureAISearchIndexSettingsService;

    internal IStringLocalizer S;

    public AzureAISearchIndexSettingsStep(
        AzureAISearchIndexManager indexManager,
        AzureAIIndexDocumentManager azureAIIndexDocumentManager,
        AzureAISearchIndexSettingsService azureAISearchIndexSettingsService,
        IStringLocalizer<AzureAISearchIndexSettingsStep> stringLocalizer)
        : base(Name)
    {
        _indexManager = indexManager;
        _azureAIIndexDocumentManager = azureAIIndexDocumentManager;
        _azureAISearchIndexSettingsService = azureAISearchIndexSettingsService;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step["Indices"] is not JsonArray indexes)
        {
            return;
        }

        var newIndexNames = new List<string>();

        foreach (var index in indexes)
        {
            var indexInfo = index.ToObject<AzureAISearchIndexInfo>();

            if (string.IsNullOrWhiteSpace(indexInfo.IndexName))
            {
                context.Errors.Add(S["No index name was provided in the '{0}' recipe step.", Name]);

                continue;
            }

            if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(indexInfo.IndexName, out var indexName))
            {
                context.Errors.Add(S["Invalid index name was provided in the recipe step. IndexName: {0}.", indexInfo.IndexName]);

                continue;
            }

            if (indexInfo.IndexedContentTypes == null || indexInfo.IndexedContentTypes.Length == 0)
            {
                context.Errors.Add(S["No {0} were provided in the recipe step. IndexName: {1}.", nameof(indexInfo.IndexedContentTypes), indexInfo.IndexName]);

                continue;
            }

            if (!await _indexManager.ExistsAsync(indexInfo.IndexName))
            {
                var indexSettings = new AzureAISearchIndexSettings()
                {
                    IndexName = indexInfo.IndexName,
                    AnalyzerName = indexInfo.AnalyzerName,
                    QueryAnalyzerName = indexInfo.QueryAnalyzerName,
                    IndexedContentTypes = indexInfo.IndexedContentTypes,
                    IndexLatest = indexInfo.IndexLatest,
                    Culture = indexInfo.Culture,
                };

                if (string.IsNullOrWhiteSpace(indexSettings.AnalyzerName))
                {
                    indexSettings.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(indexSettings.QueryAnalyzerName))
                {
                    indexSettings.QueryAnalyzerName = indexSettings.AnalyzerName;
                }

                indexSettings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(indexSettings);
                indexSettings.IndexFullName = _indexManager.GetFullIndexName(indexSettings.IndexName);

                if (await _indexManager.CreateAsync(indexSettings))
                {
                    await _azureAISearchIndexSettingsService.UpdateAsync(indexSettings);

                    newIndexNames.Add(indexSettings.IndexName);
                }
            }
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(AzureAISearchIndexRebuildDeploymentSource.Name, async scope =>
        {
            var searchIndexingService = scope.ServiceProvider.GetService<AzureAISearchIndexingService>();

            await searchIndexingService.ProcessContentItemsAsync(newIndexNames.ToArray());
        });
    }
}
