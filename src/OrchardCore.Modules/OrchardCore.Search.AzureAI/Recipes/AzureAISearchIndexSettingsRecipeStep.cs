using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexSettingsRecipeStep : RecipeImportStep<object>
{
    public const string StepName = "azureai-index-create";

    private readonly IIndexProfileManager _indexProfileManager;

    internal readonly IStringLocalizer S;

    public AzureAISearchIndexSettingsRecipeStep(
        IIndexProfileManager indexProfileManager,
        IStringLocalizer<AzureAISearchIndexSettingsRecipeStep> stringLocalizer)
    {
        _indexProfileManager = indexProfileManager;
        S = stringLocalizer;
    }

    public override string Name => StepName;

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Azure AI Search Index Create")
            .Description("Creates or updates Azure AI Search indexes.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Indices", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(object model, RecipeExecutionContext context)
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
