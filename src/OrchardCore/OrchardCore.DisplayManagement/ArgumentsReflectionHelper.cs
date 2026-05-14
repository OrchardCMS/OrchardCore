using System.Collections.Concurrent;
using System.Reflection;

namespace OrchardCore.DisplayManagement;

/// <summary>
/// Internal helper class that provides cached reflection-based property access for types
/// that don't implement <see cref="INamedEnumerable{T}"/>.
/// </summary>
internal static class ArgumentsReflectionHelper
{
    private static readonly ConcurrentDictionary<Type, Func<object, INamedEnumerable<object>>> _propertiesAccessors = new();

    /// <summary>
    /// Creates an <see cref="INamedEnumerable{T}"/> from an object's properties using cached reflection.
    /// </summary>
    /// <remarks>
    /// This method is slower than using <see cref="INamedEnumerable{T}"/> but provides a fallback
    /// for anonymous types and other types where compile-time generation isn't possible.
    /// </remarks>
    public static INamedEnumerable<object> FromReflection(object propertyObject)
    {
        var propertiesAccessor = _propertiesAccessors.GetOrAdd(propertyObject.GetType(), type =>
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var names = new string[properties.Length];

            for (var i = 0; i < properties.Length; i++)
            {
                names[i] = properties[i].Name;
            }

            return obj =>
            {
                var values = new object[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(obj, null);
                }
                return Arguments.From(values, names);
            };
        });

        return propertiesAccessor(propertyObject);
    }
}
