namespace System.Text.Json.Serialization;

/// <summary>
/// Marker interface for concrete fallback types that represent unrecognized polymorphic types
/// during JSON deserialization. Implementations preserve the original type discriminator and
/// raw JSON data, enabling round-trip serialization without data loss.
/// </summary>
public interface IUnknownTypePlaceholder
{
    /// <summary>
    /// Gets or sets the original type discriminator value from the JSON payload.
    /// </summary>
    string TypeDiscriminator { get; set; }

    /// <summary>
    /// Gets or sets the original raw JSON data, enabling round-trip serialization.
    /// </summary>
    JsonElement RawData { get; set; }
}
