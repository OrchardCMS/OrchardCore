using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexSettingsStep : NamedRecipeStepHandler
{
    public const string Name = "azureai-index-create";

    private readonly AzureAISearchIndexManager _indexManager;
    private readonly AzureAISearchIndexSettingsService _azureAISearchIndexSettingsService;

    internal readonly IStringLocalizer S;

    public AzureAISearchIndexSettingsStep(
        AzureAISearchIndexManager indexManager,
        AzureAISearchIndexSettingsService azureAISearchIndexSettingsService,
        IStringLocalizer<AzureAISearchIndexSettingsStep> stringLocalizer)
        : base(Name)
    {
        _indexManager = indexManager;
        _azureAISearchIndexSettingsService = azureAISearchIndexSettingsService;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step["Indices"] is not JsonArray tokens)
        {
            return;
        }

        foreach (var token in tokens)
        {
            var sourceName = token[nameof(AzureAISearchIndexSettings.Source)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(sourceName))
            {
                context.Errors.Add(S["Could not find provider name. The deployment will not be imported."]);

                continue;
            }

            var indexName = token[nameof(AzureAISearchIndexSettings.IndexName)]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(indexName))
            {
                context.Errors.Add(S["No index name was provided in the '{0}' recipe step.", Name]);

                continue;
            }

            if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(indexName, out var safeIndexName))
            {
                context.Errors.Add(S["Invalid index name was provided in the recipe step. IndexName: {0}.", indexName]);

                continue;
            }

            if (!await _indexManager.ExistsAsync(safeIndexName))
            {
                var indexSettings = await _azureAISearchIndexSettingsService.NewAsync(sourceName, token);
                indexSettings.IndexFullName = _indexManager.GetFullIndexName(indexName);
                await _azureAISearchIndexSettingsService.SetMappingsAsync(indexSettings);

                var validationResult = await _azureAISearchIndexSettingsService.ValidateAsync(indexSettings);

                if (!validationResult.Succeeded)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        context.Errors.Add(error.ErrorMessage);
                    }

                    continue;
                }

                if (await _indexManager.CreateAsync(indexSettings))
                {
                    await _azureAISearchIndexSettingsService.UpdateAsync(indexSettings);

                    await _azureAISearchIndexSettingsService.SynchronizeAsync(indexSettings);
                }
            }
        }
    }
}
