using OrchardCore.Environment.Shell;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Defines a recipe descriptor that provides metadata and schema for a recipe.
/// This interface abstracts the source of the recipe, allowing recipes to be defined
/// from files, code, or other sources.
/// </summary>
public interface IRecipeDescriptor
{
    /// <summary>
    /// Gets the unique internal name of the recipe.
    /// Used for identifying the recipe in code, including when executing it from other recipes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the friendly display name shown in the admin UI or during setup.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets a short description of what the recipe does.
    /// Displayed in the admin and setup UIs.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the name of the recipe creator or organization.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Gets the URL to the website or documentation for the recipe.
    /// </summary>
    string WebSite { get; }

    /// <summary>
    /// Gets the semantic version (e.g., "1.0.0") representing the recipe version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets a value indicating whether this recipe should be available during tenant setup.
    /// </summary>
    bool IsSetupRecipe { get; }

    /// <summary>
    /// Gets the UTC date and time when the recipe was exported, if applicable.
    /// </summary>
    DateTime? ExportUtc { get; }

    /// <summary>
    /// Gets the categories for organizing recipes in the UI.
    /// </summary>
    string[] Categories { get; }

    /// <summary>
    /// Gets keywords for categorizing the recipe in the UI (e.g., "blog", "theme").
    /// </summary>
    string[] Tags { get; }

    /// <summary>
    /// Gets a value indicating whether the recipe should be executed in a new scope.
    /// Defaults to <c>true</c>.
    /// </summary>
    bool RequireNewScope { get; }

    /// <summary>
    /// Opens a stream to read the recipe content.
    /// </summary>
    /// <returns>A stream containing the recipe JSON content.</returns>
    Task<Stream> OpenReadStreamAsync();

    bool IsAvailable(ShellSettings shellSettings);
}
