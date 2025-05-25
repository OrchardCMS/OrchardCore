using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexSettingsStep : NamedRecipeStepHandler
{
    public const string Name = "azureai-index-create";

    private readonly IIndexEntityManager _indexEntityManager;

    internal readonly IStringLocalizer S;

    public AzureAISearchIndexSettingsStep(
        IIndexEntityManager indexEntityManager,
        IStringLocalizer<AzureAISearchIndexSettingsStep> stringLocalizer)
        : base(Name)
    {
        _indexEntityManager = indexEntityManager;
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
            IndexEntity index = null;

            var id = token[nameof(index.Id)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(id))
            {
                index = await _indexEntityManager.FindByIdAsync(id);
            }

            if (index is not null)
            {
                await _indexEntityManager.UpdateAsync(index, token);
            }
            else
            {
                var type = token[nameof(index.Type)]?.GetValue<string>() ?? IndexingConstants.ContentsIndexSource;

                index = await _indexEntityManager.NewAsync(AzureAISearchConstants.ProviderName, type, token);
            }

            var validationResult = await _indexEntityManager.ValidateAsync(index);

            if (!validationResult.Succeeded)
            {
                foreach (var error in validationResult.Errors)
                {
                    context.Errors.Add(error.ErrorMessage);
                }

                continue;
            }

            await _indexEntityManager.CreateAsync(index);
        }
    }
}
