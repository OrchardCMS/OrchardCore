namespace OrchardCore.ContentTypes.Management;

/// <summary>
/// Contributes module-specific content definition settings contracts to remote management discovery.
/// </summary>
public interface IContentDefinitionManagementSchemaProvider
{
    /// <summary>
    /// Gets the settings contracts contributed by the module.
    /// </summary>
    IEnumerable<ContentDefinitionManagementSchema> GetSchemas();
}

/// <summary>
/// Describes a module-specific content definition settings contract.
/// </summary>
public sealed class ContentDefinitionManagementSchema
{
    /// <summary>
    /// Gets or sets the JSON settings object name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the CLR type used to generate the JSON Schema.
    /// </summary>
    public required Type Type { get; init; }

    /// <summary>
    /// Gets or sets the definition scope where the settings object is valid.
    /// </summary>
    public required ContentDefinitionManagementSchemaScope Scope { get; init; }

    /// <summary>
    /// Gets or sets the part or field type to which the settings apply.
    /// </summary>
    public string AppliesTo { get; init; }
}

/// <summary>
/// Identifies the content definition level that accepts a settings contract.
/// </summary>
public enum ContentDefinitionManagementSchemaScope
{
    /// <summary>
    /// The settings apply to a content type.
    /// </summary>
    ContentType,

    /// <summary>
    /// The settings apply to a part attached to a content type.
    /// </summary>
    ContentTypePart,

    /// <summary>
    /// The settings apply to a content part definition.
    /// </summary>
    ContentPart,

    /// <summary>
    /// The settings apply to a field attached to a content part.
    /// </summary>
    ContentPartField,
}
