using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class AzureAISearchIndexSettingsStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    public const string Name = "azureai-index-create";

    private readonly IIndexProfileManager _indexProfileManager;

    internal readonly IStringLocalizer S;

    public AzureAISearchIndexSettingsStep(
        IIndexProfileManager indexProfileManager,
        IStringLocalizer<AzureAISearchIndexSettingsStep> stringLocalizer)
        : base(Name)
    {
        _indexProfileManager = indexProfileManager;
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
            IndexProfile index = null;

            var id = token[nameof(index.Id)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(id))
            {
                index = await _indexProfileManager.FindByIdAsync(id);
            }

            if (index is not null)
            {
                await _indexProfileManager.UpdateAsync(index, token);
            }
            else
            {
                var type = token[nameof(index.Type)]?.GetValue<string>() ?? IndexingConstants.ContentsIndexSource;

                index = await _indexProfileManager.NewAsync(AzureAISearchConstants.ProviderName, type, token);
            }

            var validationResult = await _indexProfileManager.ValidateAsync(index);

            if (!validationResult.Succeeded)
            {
                foreach (var error in validationResult.Errors)
                {
                    context.Errors.Add(error.ErrorMessage);
                }

                continue;
            }

            await _indexProfileManager.CreateAsync(index);
        }
    }
}
