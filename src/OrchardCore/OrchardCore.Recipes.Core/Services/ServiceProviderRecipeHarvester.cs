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
    public Task<IEnumerable<Models.RecipeDescriptor>> HarvestRecipesAsync()
    {
        var descriptors = _recipeDescriptors
            .Select(ToRecipeDescriptor)
            .ToList();

        return Task.FromResult<IEnumerable<Models.RecipeDescriptor>>(descriptors);
    }

    /// <summary>
    /// Converts an <see cref="IRecipeDescriptor"/> to the legacy <see cref="Models.RecipeDescriptor"/> format
    /// for backward compatibility with existing infrastructure.
    /// </summary>
    private static Models.RecipeDescriptor ToRecipeDescriptor(IRecipeDescriptor descriptor)
    {
        return new CodeRecipeDescriptorAdapter(descriptor);
    }
}
