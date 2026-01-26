namespace OrchardCore.Recipes.Services;

/// <summary>
/// A recipe harvester that collects recipes from <see cref="IRecipeDescriptor"/> instances
/// registered in the dependency injection container.
/// This allows recipes to be defined in code and registered via <c>AddRecipe&lt;T&gt;</c>.
/// </summary>
public sealed class ServiceProviderRecipeHarvester : IRecipeHarvester
{
    private readonly IEnumerable<IRecipeDescriptor> _recipeDescriptors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProviderRecipeHarvester"/> class.
    /// </summary>
    /// <param name="recipeDescriptors">The collection of registered recipe descriptors.</param>
    public ServiceProviderRecipeHarvester(IEnumerable<IRecipeDescriptor> recipeDescriptors)
    {
        _recipeDescriptors = recipeDescriptors ?? [];
    }

    /// <inheritdoc />
    public Task<IEnumerable<IRecipeDescriptor>> HarvestRecipesAsync()
    {
        return Task.FromResult(_recipeDescriptors);
    }
}
