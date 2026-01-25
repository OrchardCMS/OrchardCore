using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Decorates recipe harvesters to apply filters.
/// </summary>
public sealed class FilteredRecipeHarvester : IRecipeHarvester
{
    private readonly IEnumerable<IRecipeHarvester> _harvesters;
    private readonly IEnumerable<IRecipeFilter> _filters;

    public FilteredRecipeHarvester(
        IEnumerable<IRecipeHarvester> harvesters,
        IEnumerable<IRecipeFilter> filters)
    {
        // Exclude self from the harvesters to avoid infinite recursion
        _harvesters = harvesters.Where(h => h is not FilteredRecipeHarvester);
        _filters = filters.OrderBy(f => f.Order);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
    {
        var allRecipes = new List<RecipeDescriptor>();

        foreach (var harvester in _harvesters)
        {
            var recipes = await harvester.HarvestRecipesAsync();
            allRecipes.AddRange(recipes);
        }

        if (!_filters.Any())
        {
            return allRecipes;
        }

        var filteredRecipes = new List<RecipeDescriptor>();

        foreach (var recipe in allRecipes)
        {
            var include = true;

            foreach (var filter in _filters)
            {
                if (!await filter.ShouldIncludeAsync(recipe))
                {
                    include = false;
                    break;
                }
            }

            if (include)
            {
                filteredRecipes.Add(recipe);
            }
        }

        return filteredRecipes;
    }
}
