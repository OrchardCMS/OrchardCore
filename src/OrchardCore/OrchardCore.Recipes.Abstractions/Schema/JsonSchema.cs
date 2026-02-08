using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Schema;

/// <summary>
/// Represents a JSON Schema used for recipe step validation and documentation.
/// This is a lightweight in-house implementation that uses <see cref="JsonObject"/> as the backing storage.
/// </summary>
public sealed class JsonSchema
{
    /// <summary>
    /// Gets or sets the backing JSON object for this schema.
    /// </summary>
    public JsonObject SchemaObject { get; }

    /// <summary>
    /// Creates a new instance of <see cref="JsonSchema"/> with the specified JSON object.
    /// </summary>
    /// <param name="schemaObject">The JSON object representing the schema.</param>
    public JsonSchema(JsonObject schemaObject)
    {
        ArgumentNullException.ThrowIfNull(schemaObject);
        SchemaObject = schemaObject;
    }

    /// <summary>
    /// Creates an empty schema.
    /// </summary>
    public static JsonSchema Empty => new(new JsonObject());

    /// <summary>
    /// Creates a schema that allows any value (equivalent to JSON Schema {}).
    /// </summary>
    public static JsonSchema Any => new(new JsonObject());

    /// <summary>
    /// Creates a schema that disallows any value (equivalent to JSON Schema false).
    /// </summary>
    public static JsonSchema False => new(new JsonObject { ["not"] = new JsonObject() });

    /// <summary>
    /// Converts the schema to a JSON string.
    /// </summary>
    public override string ToString()
        => SchemaObject.ToJsonString();

    /// <summary>
    /// Creates a deep copy of the schema.
    /// </summary>
    public JsonSchema Clone()
        => new(SchemaObject.DeepClone().AsObject());
}
