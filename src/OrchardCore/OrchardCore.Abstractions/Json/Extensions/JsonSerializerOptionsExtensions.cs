using System.Text.Json;

namespace OrchardCore.Json.Extensions;

public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Merges the given <see cref="JsonSerializerOptions"/> into the current options.
    /// </summary>
    public static JsonSerializerOptions Merge(this JsonSerializerOptions destination, JsonSerializerOptions source)
    {
        destination.DefaultIgnoreCondition = source.DefaultIgnoreCondition;
        destination.ReferenceHandler = source.ReferenceHandler;
        destination.ReadCommentHandling = source.ReadCommentHandling;
        destination.PropertyNameCaseInsensitive = source.PropertyNameCaseInsensitive;
        destination.AllowTrailingCommas = source.AllowTrailingCommas;
        destination.WriteIndented = source.WriteIndented;
        destination.PropertyNamingPolicy = source.PropertyNamingPolicy;
        destination.Encoder = source.Encoder;
        destination.TypeInfoResolver = source.TypeInfoResolver;
        destination.PreferredObjectCreationHandling = source.PreferredObjectCreationHandling;

        foreach (var resolver in source.TypeInfoResolverChain)
        {
            if (destination.TypeInfoResolverChain.Contains(resolver))
            {
                continue;
            }

            destination.TypeInfoResolverChain.Add(resolver);
        }

        foreach (var converter in source.Converters)
        {
            if (destination.Converters.Contains(converter))
            {
                continue;
            }

            destination.Converters.Add(converter);
        }

        return destination;
    }
}
