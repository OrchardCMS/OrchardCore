namespace OrchardCore.Recipes.Models;

/// <summary>
/// Holds the recipes registered in the tenant container.
/// </summary>
/// <remarks>
/// Recipes are added with <see cref="ServiceCollectionExtensions.AddRecipe(Microsoft.Extensions.DependencyInjection.IServiceCollection, string)"/>
/// from the startup of a feature, so a recipe is only available while the feature registering it is enabled.
/// </remarks>
public sealed class RecipeOptions
{
    /// <summary>
    /// Gets the registered recipe files.
    /// </summary>
    public IList<RecipeRegistration> Recipes { get; } = [];
}
