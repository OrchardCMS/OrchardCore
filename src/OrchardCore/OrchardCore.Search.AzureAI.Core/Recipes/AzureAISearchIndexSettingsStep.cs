using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

public class AzureAISearchIndexSettingsStep(
    AzureAISearchIndexManager indexManager,
    AzureAIIndexDocumentManager azureAIIndexDocumentManager,
    AzureAISearchIndexSettingsService azureAISearchIndexSettingsService,
    ILogger<AzureAISearchIndexSettingsStep> logger
        ) : IRecipeStepHandler
{
    public const string Name = "azureai-index-create";

    private readonly AzureAISearchIndexManager _indexManager = indexManager;
    private readonly AzureAIIndexDocumentManager _azureAIIndexDocumentManager = azureAIIndexDocumentManager;
    private readonly AzureAISearchIndexSettingsService _azureAISearchIndexSettingsService = azureAISearchIndexSettingsService;
    private readonly ILogger _logger = logger;

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (context.Step["Indices"] is null)
        {
            return;
        }

        var indexNames = new List<string>();

        foreach (var index in context.Step["Indices"])
        {
            var indexSettings = index.ToObject<AzureAISearchIndexSettings>();

            if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(indexSettings.IndexName, out var indexName))
            {
                _logger.LogError("Invalid index name was provided in the recipe step. IndexName: {indexName}.", indexSettings.IndexName);

                continue;
            }

            indexSettings.IndexName = indexName;

            if (!await _indexManager.ExistsAsync(indexSettings.IndexName))
            {
                if (string.IsNullOrWhiteSpace(indexSettings.AnalyzerName))
                {
                    indexSettings.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(indexSettings.QueryAnalyzerName))
                {
                    indexSettings.QueryAnalyzerName = indexSettings.AnalyzerName;
                }

                if (indexSettings.IndexedContentTypes == null || indexSettings.IndexedContentTypes.Length == 0)
                {
                    _logger.LogError("No {fieldName} were provided in the recipe step. IndexName: {indexName}.", nameof(indexSettings.IndexedContentTypes), indexSettings.IndexName);

                    continue;
                }

                indexSettings.SetLastTaskId(0);
                indexSettings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(indexSettings.IndexedContentTypes);
                indexSettings.IndexFullName = _indexManager.GetFullIndexName(indexSettings.IndexName);

                if (await _indexManager.CreateAsync(indexSettings))
                {
                    await _azureAISearchIndexSettingsService.UpdateAsync(indexSettings);

                    indexNames.Add(indexSettings.IndexName);
                }
            }
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(AzureAISearchIndexRebuildDeploymentSource.Name, async scope =>
        {
            var searchIndexingService = scope.ServiceProvider.GetService<AzureAISearchIndexingService>();

            await searchIndexingService.ProcessContentItemsAsync(indexNames.ToArray());
        });
    }
}
