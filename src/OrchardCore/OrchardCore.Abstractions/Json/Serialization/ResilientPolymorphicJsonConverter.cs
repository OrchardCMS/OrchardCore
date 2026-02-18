using OrchardCore.Json;

namespace System.Text.Json.Serialization;

/// <summary>
/// A <see cref="JsonConverterFactory"/> that creates resilient polymorphic converters for abstract
/// or interface types with registered derived types. Unlike the built-in polymorphic deserialization,
/// this converter gracefully handles unrecognized type discriminators by deserializing into a
/// registered fallback type (implementing <see cref="IUnknownTypePlaceholder"/>) or returning
/// <see langword="null"/> instead of throwing a <see cref="NotSupportedException"/>.
/// </summary>
public sealed class ResilientPolymorphicJsonConverterFactory : JsonConverterFactory
{
    internal const string TypeDiscriminatorPropertyName = "$type";

    private readonly JsonDerivedTypesOptions _derivedTypesOptions;

    public ResilientPolymorphicJsonConverterFactory(JsonDerivedTypesOptions derivedTypesOptions)
    {
        _derivedTypesOptions = derivedTypesOptions;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return (typeToConvert.IsAbstract || typeToConvert.IsInterface)
            && _derivedTypesOptions.TryGetDerivedTypes(typeToConvert, out _);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        _derivedTypesOptions.TryGetDerivedTypes(typeToConvert, out var derivedTypes);

        var discriminatorToType = new Dictionary<string, Type>();
        var typeToDiscriminator = new Dictionary<Type, string>();

        foreach (var info in derivedTypes)
        {
            var discriminator = info.DerivedType.TypeDiscriminator?.ToString();

            if (discriminator != null)
            {
                discriminatorToType[discriminator] = info.DerivedType.DerivedType;
                typeToDiscriminator[info.DerivedType.DerivedType] = discriminator;
            }
        }

        _derivedTypesOptions.TryGetFallbackType(typeToConvert, out var fallbackType);

        var converterType = typeof(ResilientPolymorphicJsonConverter<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType, discriminatorToType, typeToDiscriminator, fallbackType)!;
    }
}

/// <summary>
/// A resilient polymorphic JSON converter that handles unrecognized type discriminators
/// by deserializing into a registered fallback type (implementing <see cref="IUnknownTypePlaceholder"/>)
/// or returning <see langword="null"/>. This allows the application to gracefully handle cases where a
/// feature that registered a derived type has been disabled, leaving existing data with unrecognized
/// type discriminators in the database.
/// </summary>
public sealed class ResilientPolymorphicJsonConverter<T> : JsonConverter<T> where T : class
{
    private readonly Dictionary<string, Type> _discriminatorToType;
    private readonly Dictionary<Type, string> _typeToDiscriminator;
    private readonly Type _fallbackType;

    public ResilientPolymorphicJsonConverter(
        Dictionary<string, Type> discriminatorToType,
        Dictionary<Type, string> typeToDiscriminator,
        Type fallbackType)
    {
        _discriminatorToType = discriminatorToType;
        _typeToDiscriminator = typeToDiscriminator;
        _fallbackType = fallbackType;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Unexpected token type '{reader.TokenType}' when deserializing '{typeof(T)}'.");
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(ResilientPolymorphicJsonConverterFactory.TypeDiscriminatorPropertyName, out var typeProp))
        {
            return DeserializeAsFallback(root, null);
        }

        var discriminator = typeProp.GetString();

        if (discriminator is null || !_discriminatorToType.TryGetValue(discriminator, out var derivedType))
        {
            return DeserializeAsFallback(root, discriminator);
        }

        return (T)root.Deserialize(derivedType, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        // For unknown type placeholders, write back the original raw JSON to preserve data integrity.
        if (value is IUnknownTypePlaceholder placeholder && placeholder.RawData.ValueKind == JsonValueKind.Object)
        {
            placeholder.RawData.WriteTo(writer);
            return;
        }

        var actualType = value.GetType();

        // Serialize the concrete type's properties (won't recurse because CanConvert
        // returns false for concrete types).
        using var doc = JsonSerializer.SerializeToDocument(value, actualType, options);

        writer.WriteStartObject();

        // Write the type discriminator.
        if (_typeToDiscriminator.TryGetValue(actualType, out var discriminator))
        {
            writer.WriteString(ResilientPolymorphicJsonConverterFactory.TypeDiscriminatorPropertyName, discriminator);
        }

        // Write all properties from the concrete type serialization.
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            prop.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    private T DeserializeAsFallback(JsonElement root, string discriminator)
    {
        if (_fallbackType is null)
        {
            return null;
        }

        // Deserialize into the fallback type. This populates base-type properties
        // (e.g., Id, Name) from the JSON automatically.
        var fallback = (T)root.Deserialize(_fallbackType);

        if (fallback is IUnknownTypePlaceholder placeholder)
        {
            placeholder.TypeDiscriminator = discriminator;
            placeholder.RawData = root.Clone();
        }

        return fallback;
    }
}
