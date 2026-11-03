using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// Checks that the recipes of a <see cref="RecipesStep"/> exist. Their own steps aren't validated.
/// </summary>
internal sealed class RecipesStepValidator : IRecipeStepValidator
{
    private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
    private readonly RecipeDescriptorCache _recipeDescriptorCache;

    internal readonly IStringLocalizer S;

    public RecipesStepValidator(
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        RecipeDescriptorCache recipeDescriptorCache,
        IStringLocalizer<RecipesStepValidator> stringLocalizer)
    {
        _recipeHarvesters = recipeHarvesters;
        _recipeDescriptorCache = recipeDescriptorCache;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public string StepName => "Recipes";

    /// <inheritdoc />
    public async Task ValidateAsync(RecipeValidationContext context)
    {
        // The inner recipes may enable features for the next steps.
        context.FeaturesMayChange = true;

        var step = context.Step.ToObject<RecipesStep.InternalStep>();

        if (step.Values is null)
        {
            context.AddError("recipes-no-values", S["A recipes step requires a 'Values' array."]);

            return;
        }

        var recipes = await _recipeDescriptorCache.GetAsync(_recipeHarvesters);
        foreach (var recipe in step.Values)
        {
            if (!RecipeValidationContext.IsScript(recipe.Name) && !recipes.Any(candidate => candidate.Name == recipe.Name))
            {
                context.AddError("recipe-not-found", S["No recipe named '{0}' was found.", recipe.Name]);
            }
        }

        context.AddInfo("inner-recipes-not-validated", S["The steps of the inner recipes aren't validated."]);
    }
}
