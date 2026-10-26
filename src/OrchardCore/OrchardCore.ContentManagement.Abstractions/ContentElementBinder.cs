using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OrchardCore.ContentManagement;

/// <summary>
/// Creates content elements from their <see cref="JsonObject"/> data by setting their properties directly from the
/// nodes. Deserializing a node with the serializer first writes it to a buffer that is then parsed, which for HTML
/// contents means encoding, escaping and transcoding the same text several times on each access.
/// </summary>
internal static class ContentElementBinder
{
    private static readonly ConcurrentDictionary<Type, TypeBinding> s_bindings = new();

    /// <summary>
    /// Creates an instance of <paramref name="type"/> from <paramref name="data"/>, as the serializer would do with
    /// the given options, or with the serializer itself when the type can't be bound directly.
    /// </summary>
    public static object Bind(Type type, JsonObject data, JsonSerializerOptions options)
    {
        var binding = s_bindings.GetOrAdd(type, static (type, options) => CreateBinding(type, options), options);

        if (binding.Properties is null || !ReferenceEquals(binding.Options, options))
        {
            return data.Deserialize(type, options);
        }

        var instance = binding.TypeInfo.CreateObject();

        foreach (var property in binding.Properties)
        {
            if (!TryGetProperty(data, property.Name, out var node))
            {
                continue;
            }

            if (node is null)
            {
                if (!property.AllowsNull)
                {
                    // The serializer fails on a null value for this property, let it do so.
                    return data.Deserialize(type, options);
                }

                property.Set(instance, null);
                continue;
            }

            switch (property.Kind)
            {
                case PropertyKind.String:
                    if (node is JsonValue stringValue && stringValue.TryGetValue<string>(out var s))
                    {
                        property.Set(instance, s);
                        continue;
                    }

                    break;

                case PropertyKind.Boolean:
                    if (node is JsonValue booleanValue && booleanValue.TryGetValue<bool>(out var b))
                    {
                        property.Set(instance, b);
                        continue;
                    }

                    break;

                case PropertyKind.Int32:
                    if (node is JsonValue int32Value && int32Value.TryGetValue<int>(out var i))
                    {
                        property.Set(instance, i);
                        continue;
                    }

                    break;

                case PropertyKind.Int64:
                    if (node is JsonValue int64Value && int64Value.TryGetValue<long>(out var l))
                    {
                        property.Set(instance, l);
                        continue;
                    }

                    break;

                case PropertyKind.Double:
                    if (node is JsonValue doubleValue && doubleValue.TryGetValue<double>(out var d))
                    {
                        property.Set(instance, d);
                        continue;
                    }

                    break;

                case PropertyKind.StringArray:
                    if (TryGetStrings(node, out var strings))
                    {
                        property.Set(instance, strings);
                        continue;
                    }

                    break;

                case PropertyKind.StringList:
                    if (TryGetStrings(node, out var items))
                    {
                        property.Set(instance, new List<string>(items));
                        continue;
                    }

                    break;

                case PropertyKind.Other:
                    if (property.Populatable && property.Get(instance) is not null)
                    {
                        // The serializer populates the existing value instead of replacing it.
                        return data.Deserialize(type, options);
                    }

                    break;
            }

            // Any other value is read by the serializer, with the same options, so that any custom
            // converter, number handling or type conversion is applied as if the whole object was deserialized.
            property.Set(instance, node.Deserialize(property.PropertyType, options));
        }

        return instance;
    }

    private static bool TryGetProperty(JsonObject data, string name, out JsonNode node)
    {
        if (data.TryGetPropertyValue(name, out node))
        {
            return true;
        }

        // The serializer options ignore the case of the property names.
        foreach (var (key, value) in data)
        {
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                node = value;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetStrings(JsonNode node, out string[] strings)
    {
        if (node is not JsonArray array)
        {
            strings = null;
            return false;
        }

        strings = array.Count == 0 ? [] : new string[array.Count];

        for (var i = 0; i < strings.Length; i++)
        {
            var item = array[i];

            if (item is null)
            {
                continue;
            }

            if (item is not JsonValue value || !value.TryGetValue<string>(out var s))
            {
                strings = null;
                return false;
            }

            strings[i] = s;
        }

        return true;
    }

    private static TypeBinding CreateBinding(Type type, JsonSerializerOptions options)
    {
        options.MakeReadOnly(populateMissingResolver: true);

        var typeInfo = options.GetTypeInfo(type);

        // Only plain objects whose properties are all set as the serializer would do can be bound directly.
        if (typeInfo.Kind != JsonTypeInfoKind.Object ||
            typeInfo.CreateObject is null ||
            typeInfo.PolymorphismOptions is not null ||
            typeInfo.OnDeserializing is not null ||
            typeInfo.OnDeserialized is not null ||
            typeInfo.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow ||
            typeof(IJsonOnDeserializing).IsAssignableFrom(type) ||
            typeof(IJsonOnDeserialized).IsAssignableFrom(type))
        {
            return new TypeBinding(options, typeInfo, null);
        }

        var properties = new List<PropertyBinding>(typeInfo.Properties.Count);

        foreach (var property in typeInfo.Properties)
        {
            if (property.IsExtensionData ||
                property.IsRequired ||
                property.CustomConverter is not null ||
                property.NumberHandling is not null ||
                property.ObjectCreationHandling is not null)
            {
                return new TypeBinding(options, typeInfo, null);
            }

            var propertyType = property.PropertyType;

            if (property.Set is null)
            {
                // A read-only reference property may be populated from its current value.
                if (property.Get is not null && !propertyType.IsValueType && propertyType != typeof(string))
                {
                    return new TypeBinding(options, typeInfo, null);
                }

                continue;
            }

            var ignore = property.AttributeProvider?.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: false);

            if (ignore is { Length: > 0 } &&
                ignore[0] is JsonIgnoreAttribute { Condition: JsonIgnoreCondition.Always or JsonIgnoreCondition.WhenReading })
            {
                return new TypeBinding(options, typeInfo, null);
            }

            var kind = GetKind(propertyType);
            var allowsNull = !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) is not null;

            // A reference property that already has a value is populated by the serializer instead of being replaced.
            var populatable = kind == PropertyKind.Other && !propertyType.IsValueType && property.Get is not null;

            properties.Add(new PropertyBinding(property.Name, propertyType, kind, allowsNull, populatable, property.Set, property.Get));
        }

        return new TypeBinding(options, typeInfo, properties.ToArray());
    }

    private static PropertyKind GetKind(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string))
        {
            return PropertyKind.String;
        }

        if (type == typeof(bool))
        {
            return PropertyKind.Boolean;
        }

        if (type == typeof(int))
        {
            return PropertyKind.Int32;
        }

        if (type == typeof(long))
        {
            return PropertyKind.Int64;
        }

        if (type == typeof(double))
        {
            return PropertyKind.Double;
        }

        if (type == typeof(string[]))
        {
            return PropertyKind.StringArray;
        }

        if (type == typeof(List<string>))
        {
            return PropertyKind.StringList;
        }

        return PropertyKind.Other;
    }

    private enum PropertyKind
    {
        Other,
        String,
        Boolean,
        Int32,
        Int64,
        Double,
        StringArray,
        StringList,
    }

    private sealed record TypeBinding(JsonSerializerOptions Options, JsonTypeInfo TypeInfo, PropertyBinding[] Properties);

    private sealed record PropertyBinding(
        string Name,
        Type PropertyType,
        PropertyKind Kind,
        bool AllowsNull,
        bool Populatable,
        Action<object, object> Set,
        Func<object, object> Get);
}
