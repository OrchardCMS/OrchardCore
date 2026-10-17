namespace OrchardCore.Recipes.Models;

/// <summary>
/// Represents a recipe file registered with <see cref="ServiceCollectionExtensions.AddRecipe(Microsoft.Extensions.DependencyInjection.IServiceCollection, string)"/>.
/// </summary>
public sealed class RecipeRegistration
{
    /// <summary>
    /// Creates a new instance of <see cref="RecipeRegistration"/>.
    /// </summary>
    /// <param name="path">The path of the recipe file, relative to the root of the extension, or to the content root of the application when <paramref name="extensionId"/> is <see langword="null"/>.</param>
    /// <param name="extensionId">The identifier of the extension holding the recipe file, or <see langword="null"/> for a file of the application content root.</param>
    /// <param name="featureId">The identifier of the feature that registered the recipe, or <see langword="null"/> when it was not registered by a feature.</param>
    public RecipeRegistration(string path, string extensionId, string featureId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Path = path;
        ExtensionId = extensionId;
        FeatureId = featureId;
    }

    /// <summary>
    /// Gets the path of the recipe file, relative to the root of the extension, or to the content root of the
    /// application when <see cref="ExtensionId"/> is <see langword="null"/>.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the identifier of the extension holding the recipe file, or <see langword="null"/> for a file of the
    /// application content root.
    /// </summary>
    public string ExtensionId { get; }

    /// <summary>
    /// Gets the identifier of the feature that registered the recipe, or <see langword="null"/> when it was not
    /// registered by a feature.
    /// </summary>
    public string FeatureId { get; }
}
