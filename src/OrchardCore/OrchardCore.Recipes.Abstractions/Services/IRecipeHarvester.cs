namespace OrchardCore.Recipes.Services;

public interface IRecipeHarvester
{
    /// <summary>
    /// Returns a collection of all recipes.
    /// </summary>
    Task<IEnumerable<IRecipeDescriptor>> HarvestRecipesAsync();
}
