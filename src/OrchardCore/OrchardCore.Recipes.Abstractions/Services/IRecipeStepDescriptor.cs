using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Describes a recipe step with metadata including schema information.
/// This interface enables recipe step discovery and validation.
/// </summary>
public interface IRecipeStepDescriptor
{
    /// <summary>
    /// Gets the unique name of the recipe step used in recipe JSON files.
    /// This name is used to match steps during recipe execution.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a human-readable display name for the recipe step.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets a description of what the recipe step does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the category this recipe step belongs to.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Gets the JSON schema that describes and validates this recipe step's structure.
    /// Returns <c>null</c> if no schema is available.
    /// </summary>
    /// <returns>A <see cref="JsonObject"/> representing the JSON schema, or <c>null</c>.</returns>
    ValueTask<JsonObject> GetSchemaAsync();
}
