using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentTypes.Models;

public sealed class ContentTypeDefinitionDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string DisplayName { get; set; }

    public ContentTypeDefinitionSettingsDto Settings { get; set; } = new();

    public List<ContentTypePartDefinitionDto> Parts { get; set; } = [];
}

public sealed class ContentTypePartDefinitionDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string PartName { get; set; }

    public ContentTypePartDefinitionSettingsDto Settings { get; set; } = new();
}

public sealed class ContentPartDefinitionDto
{
    [Required]
    public string Name { get; set; }

    public ContentPartDefinitionSettingsDto Settings { get; set; } = new();

    public List<ContentPartFieldDefinitionDto> Fields { get; set; } = [];
}

public sealed class ContentPartFieldDefinitionDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string FieldName { get; set; }

    public ContentPartFieldDefinitionSettingsDto Settings { get; set; } = new();
}

/// <summary>
/// Represents an extensible settings bag for a content definition.
/// </summary>
public abstract class ContentDefinitionSettingsDto
{
    /// <summary>
    /// Gets or sets settings contributed by features beyond the built-in settings contract.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalSettings { get; set; } = [];

    internal JsonObject ToJsonObject()
        => JsonSerializer.SerializeToNode(this, GetType(), JsonSerializerOptions.Default)?.AsObject() ?? [];

    internal static T FromJsonObject<T>(JsonObject settings) where T : ContentDefinitionSettingsDto, new()
        => settings.Deserialize<T>(JsonSerializerOptions.Web) ?? new T();
}

/// <summary>
/// Represents the settings attached to a content type definition.
/// </summary>
public sealed class ContentTypeDefinitionSettingsDto : ContentDefinitionSettingsDto
{
    /// <summary>
    /// Gets or sets the built-in content type settings.
    /// </summary>
    [JsonPropertyName(nameof(ContentTypeSettings))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ContentTypeSettings ContentTypeSettings { get; set; }
}

/// <summary>
/// Represents the settings attached to a part on a content type.
/// </summary>
public sealed class ContentTypePartDefinitionSettingsDto : ContentDefinitionSettingsDto
{
    /// <summary>
    /// Gets or sets the built-in attached-part settings.
    /// </summary>
    [JsonPropertyName(nameof(ContentTypePartSettings))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ContentTypePartSettings ContentTypePartSettings { get; set; }
}

/// <summary>
/// Represents the settings attached to a content part definition.
/// </summary>
public sealed class ContentPartDefinitionSettingsDto : ContentDefinitionSettingsDto
{
    /// <summary>
    /// Gets or sets the built-in content part settings.
    /// </summary>
    [JsonPropertyName(nameof(ContentPartSettings))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ContentPartSettings ContentPartSettings { get; set; }
}

/// <summary>
/// Represents the settings attached to a content part field definition.
/// </summary>
public sealed class ContentPartFieldDefinitionSettingsDto : ContentDefinitionSettingsDto
{
    /// <summary>
    /// Gets or sets the built-in content part field settings.
    /// </summary>
    [JsonPropertyName(nameof(ContentPartFieldSettings))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ContentPartFieldSettings ContentPartFieldSettings { get; set; }
}

public sealed class ContentDefinitionPartTypeDescriptor
{
    public string Name { get; set; }

    public bool Attachable { get; set; }

    public bool Reusable { get; set; }

    public JsonObject Schema { get; set; } = [];
}

public sealed class ContentDefinitionFieldTypeDescriptor
{
    public string Name { get; set; }

    public JsonObject Schema { get; set; } = [];
}

/// <summary>
/// Describes a module-specific content definition settings contract.
/// </summary>
public sealed class ContentDefinitionSettingsSchemaDto
{
    /// <summary>
    /// Gets the JSON settings object name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the content definition scope where the settings object is valid.
    /// </summary>
    public string Scope { get; init; }

    /// <summary>
    /// Gets the part or field type to which the settings apply.
    /// </summary>
    public string AppliesTo { get; init; }

    /// <summary>
    /// Gets the JSON Schema for the settings object value.
    /// </summary>
    public JsonObject Schema { get; init; } = [];
}

/// <summary>
/// Represents a paged list of content-definition resources.
/// </summary>
public sealed class ContentDefinitionListResponse<T>
{
    /// <summary>
    /// Gets the number of resources skipped.
    /// </summary>
    public int Skip { get; init; }

    /// <summary>
    /// Gets the maximum number of resources requested.
    /// </summary>
    public int Take { get; init; }

    /// <summary>
    /// Gets the total number of matching resources.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the resources in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];
}
