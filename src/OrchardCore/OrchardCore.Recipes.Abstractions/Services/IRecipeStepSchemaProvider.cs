using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Provides JSON schema for a recipe step.
/// Implementations can provide schemas from code, files, or other sources.
/// </summary>
public interface IRecipeStepSchemaProvider
{
    /// <summary>
    /// Gets the name of the recipe step this provider supplies a schema for.
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Gets the JSON schema for the recipe step.
    /// </summary>
    /// <returns>A <see cref="JsonObject"/> representing the JSON schema, or <c>null</c> if no schema is available.</returns>
    ValueTask<JsonObject> GetSchemaAsync();
}
