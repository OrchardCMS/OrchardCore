using Json.Schema;

namespace OrchardCore.ContentManagement.Metadata;

/// <summary>
/// Allows content fields to contribute JSON Schema definitions for their settings.
/// </summary>
public interface IContentFieldSchemaHandler
{
    /// <summary>
    /// Builds the JSON Schema for this content field's settings.
    /// </summary>
    /// <returns>The JSON Schema for the field settings, or null if no schema is available.</returns>
    JsonSchema GetSettingsSchema();
}
