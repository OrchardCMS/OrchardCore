using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OrchardCore.Localization;

namespace OrchardCore.Json.Serialization;

/// <summary>
/// Removes the properties that hold a <see cref="LocalizationSource"/> from the JSON contract of a type, so a source is
/// never serialized or deserialized.
/// </summary>
/// <remarks>
/// A source is a static declaration owned by code, not data: it names a <see cref="Type"/>, which can't be serialized,
/// and a type can't be created safely from a name read from a document. Removing a property from
/// <see cref="JsonTypeInfo.Properties"/> excludes it from writing and from reading, and doesn't rely on every module
/// repeating <c>[JsonIgnore]</c>, which <c>System.Text.Json</c> doesn't inherit from the base property to an override.
/// Properties of an array, a collection or a dictionary of sources are removed too.
/// Add it to the <see cref="DefaultJsonTypeInfoResolver.Modifiers"/> of a resolver, or call
/// <see cref="OrchardCore.Json.Extensions.JsonSerializerOptionsExtensions.IgnoreLocalizationSources(JsonSerializerOptions)"/>.
/// </remarks>
public static class LocalizationSourceJsonModifier
{
    /// <summary>
    /// Removes the properties that hold a <see cref="LocalizationSource"/> from the given type info.
    /// </summary>
    /// <param name="typeInfo">The contract of the type being configured.</param>
    public static void Ignore(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
        {
            if (HoldsLocalizationSource(typeInfo.Properties[i].PropertyType))
            {
                typeInfo.Properties.RemoveAt(i);
            }
        }
    }

    private static bool HoldsLocalizationSource(Type type)
    {
        if (type == typeof(LocalizationSource))
        {
            return true;
        }

        if (type.IsArray)
        {
            return HoldsLocalizationSource(type.GetElementType());
        }

        return type.IsGenericType && type.GetGenericArguments().Any(HoldsLocalizationSource);
    }
}
