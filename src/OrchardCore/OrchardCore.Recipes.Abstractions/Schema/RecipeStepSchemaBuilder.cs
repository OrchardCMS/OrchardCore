using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Schema;

/// <summary>
/// Builder for creating <see cref="JsonSchema"/> instances.
/// Provides a fluent API for defining JSON Schema structures.
/// </summary>
public sealed class RecipeStepSchemaBuilder
{
    private readonly JsonObject _schema = new();
    private JsonObject _properties;
    private JsonArray _required;
    private JsonArray _allOf;
    private JsonArray _anyOf;
    private JsonArray _oneOf;

    /// <summary>
    /// Sets the $schema meta-schema URL.
    /// </summary>
    /// <param name="schemaUri">The meta-schema URI (e.g., "https://json-schema.org/draft/2020-12/schema").</param>
    public RecipeStepSchemaBuilder Schema(string schemaUri)
    {
        _schema["$schema"] = schemaUri;
        return this;
    }

    /// <summary>
    /// Sets the $schema to Draft 2020-12.
    /// </summary>
    public RecipeStepSchemaBuilder SchemaDraft202012()
        => Schema("https://json-schema.org/draft/2020-12/schema");

    /// <summary>
    /// Sets the $id for the schema.
    /// </summary>
    /// <param name="id">The schema ID.</param>
    public RecipeStepSchemaBuilder Id(string id)
    {
        _schema["$id"] = id;
        return this;
    }

    /// <summary>
    /// Sets the type of the schema.
    /// </summary>
    /// <param name="type">The JSON Schema type (e.g., "object", "array", "string", etc.).</param>
    public RecipeStepSchemaBuilder Type(string type)
    {
        _schema["type"] = type;
        return this;
    }

    /// <summary>
    /// Sets the type to "object".
    /// </summary>
    public RecipeStepSchemaBuilder TypeObject() => Type("object");

    /// <summary>
    /// Sets the type to "array".
    /// </summary>
    public RecipeStepSchemaBuilder TypeArray() => Type("array");

    /// <summary>
    /// Sets the type to "string".
    /// </summary>
    public RecipeStepSchemaBuilder TypeString() => Type("string");

    /// <summary>
    /// Sets the type to "integer".
    /// </summary>
    public RecipeStepSchemaBuilder TypeInteger() => Type("integer");

    /// <summary>
    /// Sets the type to "number".
    /// </summary>
    public RecipeStepSchemaBuilder TypeNumber() => Type("number");

    /// <summary>
    /// Sets the type to "boolean".
    /// </summary>
    public RecipeStepSchemaBuilder TypeBoolean() => Type("boolean");

    /// <summary>
    /// Sets the type to "null".
    /// </summary>
    public RecipeStepSchemaBuilder TypeNull() => Type("null");

    /// <summary>
    /// Sets the title of the schema.
    /// </summary>
    /// <param name="title">The title.</param>
    public RecipeStepSchemaBuilder Title(string title)
    {
        _schema["title"] = title;
        return this;
    }

    /// <summary>
    /// Sets the description of the schema.
    /// </summary>
    /// <param name="description">The description.</param>
    public RecipeStepSchemaBuilder Description(string description)
    {
        _schema["description"] = description;
        return this;
    }

    /// <summary>
    /// Sets the default value.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    public RecipeStepSchemaBuilder Default(JsonNode defaultValue)
    {
        _schema["default"] = defaultValue?.DeepClone();
        return this;
    }

    /// <summary>
    /// Sets the const value (exact match).
    /// </summary>
    /// <param name="value">The const value.</param>
    public RecipeStepSchemaBuilder Const(string value)
    {
        _schema["const"] = value;
        return this;
    }

    /// <summary>
    /// Sets the const value (exact match) from a JsonNode.
    /// </summary>
    /// <param name="value">The const value.</param>
    public RecipeStepSchemaBuilder Const(JsonNode value)
    {
        _schema["const"] = value?.DeepClone();
        return this;
    }

    /// <summary>
    /// Sets the enum values.
    /// </summary>
    /// <param name="values">The allowed values.</param>
    public RecipeStepSchemaBuilder Enum(params string[] values)
    {
        var enumArray = new JsonArray();
        foreach (var value in values)
        {
            enumArray.Add(value);
        }
        _schema["enum"] = enumArray;
        return this;
    }

    /// <summary>
    /// Sets the enum values from a collection.
    /// </summary>
    /// <param name="values">The allowed values.</param>
    public RecipeStepSchemaBuilder Enum(IEnumerable<string> values)
        => Enum(values.ToArray());

    /// <summary>
    /// Adds a property to the schema.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="propertySchema">The property schema builder.</param>
    public RecipeStepSchemaBuilder Property(string name, RecipeStepSchemaBuilder propertySchema)
    {
        _properties ??= new JsonObject();
        _properties[name] = propertySchema.Build().SchemaObject.DeepClone();
        return this;
    }

    /// <summary>
    /// Adds a property to the schema.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="propertySchema">The property schema.</param>
    public RecipeStepSchemaBuilder Property(string name, JsonSchema propertySchema)
    {
        _properties ??= new JsonObject();
        _properties[name] = propertySchema.SchemaObject.DeepClone();
        return this;
    }

    /// <summary>
    /// Adds multiple properties to the schema.
    /// </summary>
    /// <param name="properties">Tuples of property names and their schema builders.</param>
    public RecipeStepSchemaBuilder Properties(params (string name, RecipeStepSchemaBuilder schema)[] properties)
    {
        foreach (var (name, schema) in properties)
        {
            Property(name, schema);
        }
        return this;
    }

    /// <summary>
    /// Adds multiple properties to the schema.
    /// </summary>
    /// <param name="properties">Tuples of property names and their schemas.</param>
    public RecipeStepSchemaBuilder Properties(params (string name, JsonSchema schema)[] properties)
    {
        foreach (var (name, schema) in properties)
        {
            Property(name, schema);
        }
        return this;
    }

    /// <summary>
    /// Sets the required properties.
    /// </summary>
    /// <param name="propertyNames">The names of required properties.</param>
    public RecipeStepSchemaBuilder Required(params string[] propertyNames)
    {
        _required ??= new JsonArray();
        foreach (var name in propertyNames)
        {
            if (!_required.Any(n => n?.GetValue<string>() == name))
            {
                _required.Add(name);
            }
        }
        return this;
    }

    /// <summary>
    /// Sets the items schema for arrays.
    /// </summary>
    /// <param name="itemsSchema">The schema for array items.</param>
    public RecipeStepSchemaBuilder Items(RecipeStepSchemaBuilder itemsSchema)
    {
        _schema["items"] = itemsSchema.Build().SchemaObject.DeepClone();
        return this;
    }

    /// <summary>
    /// Sets the items schema for arrays.
    /// </summary>
    /// <param name="itemsSchema">The schema for array items.</param>
    public RecipeStepSchemaBuilder Items(JsonSchema itemsSchema)
    {
        _schema["items"] = itemsSchema.SchemaObject.DeepClone();
        return this;
    }

    /// <summary>
    /// Sets the minimum number of items for arrays.
    /// </summary>
    /// <param name="minItems">The minimum number of items.</param>
    public RecipeStepSchemaBuilder MinItems(int minItems)
    {
        _schema["minItems"] = minItems;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of items for arrays.
    /// </summary>
    /// <param name="maxItems">The maximum number of items.</param>
    public RecipeStepSchemaBuilder MaxItems(int maxItems)
    {
        _schema["maxItems"] = maxItems;
        return this;
    }

    /// <summary>
    /// Sets the additionalProperties constraint.
    /// </summary>
    /// <param name="allowed">Whether additional properties are allowed.</param>
    public RecipeStepSchemaBuilder AdditionalProperties(bool allowed)
    {
        _schema["additionalProperties"] = allowed;
        return this;
    }

    /// <summary>
    /// Sets the additionalProperties schema.
    /// </summary>
    /// <param name="schema">The schema for additional properties.</param>
    public RecipeStepSchemaBuilder AdditionalProperties(RecipeStepSchemaBuilder schema)
    {
        _schema["additionalProperties"] = schema.Build().SchemaObject.DeepClone();
        return this;
    }

    /// <summary>
    /// Sets the additionalProperties schema.
    /// </summary>
    /// <param name="schema">The schema for additional properties.</param>
    public RecipeStepSchemaBuilder AdditionalProperties(JsonSchema schema)
    {
        _schema["additionalProperties"] = schema.SchemaObject.DeepClone();
        return this;
    }

    /// <summary>
    /// Sets the unevaluatedProperties constraint.
    /// </summary>
    /// <param name="allowed">Whether unevaluated properties are allowed.</param>
    public RecipeStepSchemaBuilder UnevaluatedProperties(bool allowed)
    {
        _schema["unevaluatedProperties"] = allowed;
        return this;
    }

    /// <summary>
    /// Adds an allOf schema constraint.
    /// </summary>
    /// <param name="schemas">The schemas that must all match.</param>
    public RecipeStepSchemaBuilder AllOf(params RecipeStepSchemaBuilder[] schemas)
    {
        _allOf ??= new JsonArray();
        foreach (var schema in schemas)
        {
            _allOf.Add(schema.Build().SchemaObject.DeepClone());
        }
        return this;
    }

    /// <summary>
    /// Adds an allOf schema constraint.
    /// </summary>
    /// <param name="schemas">The schemas that must all match.</param>
    public RecipeStepSchemaBuilder AllOf(params JsonSchema[] schemas)
    {
        _allOf ??= new JsonArray();
        foreach (var schema in schemas)
        {
            _allOf.Add(schema.SchemaObject.DeepClone());
        }
        return this;
    }

    /// <summary>
    /// Adds an anyOf schema constraint.
    /// </summary>
    /// <param name="schemas">The schemas where at least one must match.</param>
    public RecipeStepSchemaBuilder AnyOf(params RecipeStepSchemaBuilder[] schemas)
    {
        _anyOf ??= new JsonArray();
        foreach (var schema in schemas)
        {
            _anyOf.Add(schema.Build().SchemaObject.DeepClone());
        }
        return this;
    }

    /// <summary>
    /// Adds an anyOf schema constraint.
    /// </summary>
    /// <param name="schemas">The schemas where at least one must match.</param>
    public RecipeStepSchemaBuilder AnyOf(params JsonSchema[] schemas)
    {
        _anyOf ??= new JsonArray();
        foreach (var schema in schemas)
        {
            _anyOf.Add(schema.SchemaObject.DeepClone());
        }
        return this;
    }

    /// <summary>
    /// Adds an anyOf schema constraint from a list.
    /// </summary>
    /// <param name="schemas">The schemas where at least one must match.</param>
    public RecipeStepSchemaBuilder AnyOf(IEnumerable<JsonSchema> schemas)
    {
        _anyOf ??= new JsonArray();
        foreach (var schema in schemas)
        {
            _anyOf.Add(schema.SchemaObject.DeepClone());
        }
        return this;
    }

    /// <summary>
    /// Adds a oneOf schema constraint.
    /// </summary>
    /// <param name="schemas">The schemas where exactly one must match.</param>
    public RecipeStepSchemaBuilder OneOf(params RecipeStepSchemaBuilder[] schemas)
    {
        _oneOf ??= new JsonArray();
        foreach (var schema in schemas)
        {
            _oneOf.Add(schema.Build().SchemaObject.DeepClone());
        }
        return this;
    }

    /// <summary>
    /// Adds a oneOf schema constraint.
    /// </summary>
    /// <param name="schemas">The schemas where exactly one must match.</param>
    public RecipeStepSchemaBuilder OneOf(params JsonSchema[] schemas)
    {
        _oneOf ??= new JsonArray();
        foreach (var schema in schemas)
        {
            _oneOf.Add(schema.SchemaObject.DeepClone());
        }
        return this;
    }

    /// <summary>
    /// Sets the minimum value for numbers.
    /// </summary>
    /// <param name="minimum">The minimum value.</param>
    public RecipeStepSchemaBuilder Minimum(decimal minimum)
    {
        _schema["minimum"] = minimum;
        return this;
    }

    /// <summary>
    /// Sets the maximum value for numbers.
    /// </summary>
    /// <param name="maximum">The maximum value.</param>
    public RecipeStepSchemaBuilder Maximum(decimal maximum)
    {
        _schema["maximum"] = maximum;
        return this;
    }

    /// <summary>
    /// Sets the minimum length for strings.
    /// </summary>
    /// <param name="minLength">The minimum length.</param>
    public RecipeStepSchemaBuilder MinLength(int minLength)
    {
        _schema["minLength"] = minLength;
        return this;
    }

    /// <summary>
    /// Sets the maximum length for strings.
    /// </summary>
    /// <param name="maxLength">The maximum length.</param>
    public RecipeStepSchemaBuilder MaxLength(int maxLength)
    {
        _schema["maxLength"] = maxLength;
        return this;
    }

    /// <summary>
    /// Sets the pattern for strings.
    /// </summary>
    /// <param name="pattern">The regex pattern.</param>
    public RecipeStepSchemaBuilder Pattern(string pattern)
    {
        _schema["pattern"] = pattern;
        return this;
    }

    /// <summary>
    /// Sets the format for strings.
    /// </summary>
    /// <param name="format">The format (e.g., "date-time", "uri", "email").</param>
    public RecipeStepSchemaBuilder Format(string format)
    {
        _schema["format"] = format;
        return this;
    }

    /// <summary>
    /// Sets the minimum number of properties for objects.
    /// </summary>
    /// <param name="minProperties">The minimum number of properties.</param>
    public RecipeStepSchemaBuilder MinProperties(int minProperties)
    {
        _schema["minProperties"] = minProperties;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of properties for objects.
    /// </summary>
    /// <param name="maxProperties">The maximum number of properties.</param>
    public RecipeStepSchemaBuilder MaxProperties(int maxProperties)
    {
        _schema["maxProperties"] = maxProperties;
        return this;
    }

    /// <summary>
    /// Builds and returns the <see cref="JsonSchema"/>.
    /// </summary>
    /// <returns>The built schema.</returns>
    public JsonSchema Build()
    {
        var result = _schema.DeepClone().AsObject();

        if (_properties is not null && _properties.Count > 0)
        {
            result["properties"] = _properties.DeepClone();
        }

        if (_required is not null && _required.Count > 0)
        {
            result["required"] = _required.DeepClone();
        }

        if (_allOf is not null && _allOf.Count > 0)
        {
            result["allOf"] = _allOf.DeepClone();
        }

        if (_anyOf is not null && _anyOf.Count > 0)
        {
            result["anyOf"] = _anyOf.DeepClone();
        }

        if (_oneOf is not null && _oneOf.Count > 0)
        {
            result["oneOf"] = _oneOf.DeepClone();
        }

        return new JsonSchema(result);
    }
}
