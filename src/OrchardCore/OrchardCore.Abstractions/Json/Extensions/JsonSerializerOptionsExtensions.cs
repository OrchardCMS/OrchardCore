using System.Text.Json;

namespace OrchardCore.Json.Extensions;

public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Merges the given <see cref="JsonSerializerOptions"/> into the current options.
    /// </summary>
    public static JsonSerializerOptions Merge(this JsonSerializerOptions destenation, JsonSerializerOptions source)
    {
        destenation.DefaultIgnoreCondition = source.DefaultIgnoreCondition;
        destenation.ReferenceHandler = source.ReferenceHandler;
        destenation.ReadCommentHandling = source.ReadCommentHandling;
        destenation.PropertyNameCaseInsensitive = source.PropertyNameCaseInsensitive;
        destenation.AllowTrailingCommas = source.AllowTrailingCommas;
        destenation.WriteIndented = source.WriteIndented;
        destenation.PropertyNamingPolicy = source.PropertyNamingPolicy;
        destenation.Encoder = source.Encoder;
        destenation.TypeInfoResolver = source.TypeInfoResolver;

        foreach (var resolver in source.TypeInfoResolverChain)
        {
            if (destenation.TypeInfoResolverChain.Contains(resolver))
            {
                continue;
            }

            destenation.TypeInfoResolverChain.Add(resolver);
        }

        foreach (var converter in source.Converters)
        {
            if (destenation.Converters.Contains(converter))
            {
                continue;
            }

            destenation.Converters.Add(converter);
        }

        return destenation;
    }
}
