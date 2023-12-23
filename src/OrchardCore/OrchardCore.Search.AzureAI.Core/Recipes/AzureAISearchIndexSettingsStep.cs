using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

public class AzureAISearchIndexSettingsStep(
    AzureAISearchIndexManager indexManager,
    ILogger<AzureAISearchIndexSettingsStep> logger
        ) : IRecipeStepHandler
{
    private readonly AzureAISearchIndexManager _indexManager = indexManager;
    private readonly ILogger<AzureAISearchIndexSettingsStep> _logger = logger;

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, nameof(AzureAISearchIndexSettings), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var indices = context.Step["Indices"];
        if (indices == null)
        {
            return;
        }

        foreach (var index in indices)
        {
            var elasticIndexSettings = index.ToObject<AzureAISearchIndexSettings>();

            if (!AzureAISearchIndexManager.TryGetSafeIndexName(elasticIndexSettings.IndexName, out var indexName))
            {
                _logger.LogError("Invalid index name was provided in the recipe step. IndexName: {indexName}.", elasticIndexSettings.IndexName);

                continue;
            }

            elasticIndexSettings.IndexName = indexName;

            if (!await _indexManager.ExistsAsync(elasticIndexSettings.IndexName))
            {
                if (string.IsNullOrWhiteSpace(elasticIndexSettings.AnalyzerName))
                {
                    elasticIndexSettings.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(elasticIndexSettings.QueryAnalyzerName))
                {
                    elasticIndexSettings.QueryAnalyzerName = elasticIndexSettings.AnalyzerName;
                }

                if (elasticIndexSettings.IndexedContentTypes == null || elasticIndexSettings.IndexedContentTypes.Length == 0)
                {
                    _logger.LogError("No {fieldName} were provided in the recipe step. IndexName: {indexName}.", nameof(elasticIndexSettings.IndexedContentTypes), elasticIndexSettings.IndexName);

                    continue;
                }

                await _indexManager.CreateAsync(elasticIndexSettings);
            }
        }
    }
}
