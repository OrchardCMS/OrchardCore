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

        if (context.Step["Indices"] is null)
        {
            return;
        }

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

                await _indexManager.CreateAsync(indexSettings);
            }
        }
    }
}
