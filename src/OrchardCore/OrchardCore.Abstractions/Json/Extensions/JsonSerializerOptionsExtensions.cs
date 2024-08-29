using System.Reflection;
using System.Text.Json;

namespace OrchardCore.Json.Extensions;

public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Merges the given <see cref="JsonSerializerOptions"/> into the current options.
    /// </summary>
    public static JsonSerializerOptions Merge(this JsonSerializerOptions destination, JsonSerializerOptions source)
    {
        var properties = typeof(JsonSerializerOptions)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.CanRead);

        foreach (var property in properties)
        {
            var sourceValue = property.GetValue(source);
            property.SetValue(destination, sourceValue);
        }

        MergeCollections(destination.TypeInfoResolverChain, source.TypeInfoResolverChain);
        MergeCollections(destination.Converters, source.Converters);

        // destination.PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace;

        return destination;
    }

    private static void MergeCollections<T>(IList<T> destination, IList<T> source)
    {
        foreach (var item in source)
        {
            if (!destination.Contains(item))
            {
                destination.Add(item);
            }
        }
    }
}
