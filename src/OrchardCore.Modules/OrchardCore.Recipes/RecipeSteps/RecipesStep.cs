using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// This recipe step executes a set of external recipes.
/// </summary>
public sealed class RecipesStep : NamedRecipeStepHandler
{
    private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;

    internal readonly IStringLocalizer S;

    public RecipesStep(
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        IStringLocalizer<RecipesStep> stringLocalizer)
        : base("Recipes")
    {
        _recipeHarvesters = recipeHarvesters;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var step = context.Step.ToObject<InternalStep>();

        var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(harvester => harvester.HarvestRecipesAsync()));
        var recipes = recipeCollections.SelectMany(recipe => recipe).ToDictionary(recipe => recipe.Name);

        var innerRecipes = new List<RecipeDescriptor>();
        foreach (var recipe in step.Values)
        {
            if (!recipes.TryGetValue(recipe.Name, out var value))
            {
                context.Errors.Add(S["No recipe named '{0}' was found.", recipe.Name]);

                continue;
            }

            innerRecipes.Add(value);
        }

        context.InnerRecipes = innerRecipes;
    }

    private sealed class InternalStep
    {
        public InternalStepValue[] Values { get; set; }
    }

    private sealed class InternalStepValue
    {
        public string ExecutionId { get; set; }

        public string Name { get; set; }
    }
}
