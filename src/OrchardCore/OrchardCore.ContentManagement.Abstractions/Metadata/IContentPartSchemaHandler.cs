using OrchardCore.Recipes.Schema;

namespace OrchardCore.ContentManagement.Metadata;

/// <summary>
/// Allows content parts to contribute JSON Schema definitions for their settings.
/// </summary>
public interface IContentPartSchemaHandler
{
    /// <summary>
    /// Builds the JSON Schema for this content part's settings.
    /// </summary>
    /// <returns>The JSON Schema for the part settings, or null if no schema is available.</returns>
    RecipeStepSchema GetSettingsSchema();
}
