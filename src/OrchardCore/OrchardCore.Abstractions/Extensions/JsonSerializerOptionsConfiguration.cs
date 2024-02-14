using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Extensions;

public class JsonSerializerOptionsConfiguration : IConfigureOptions<JsonSerializerOptions>
{
    private readonly IEnumerable<IJsonTypeInfoResolver> _typeInfoResolvers;
    private readonly IOptions<JsonDerivedTypesOptions> _derivedTypesOptions;

    public JsonSerializerOptionsConfiguration(
        IEnumerable<IJsonTypeInfoResolver> typeInfoResolvers,
        IOptions<JsonDerivedTypesOptions> derivedTypesOptions)
    {
        _typeInfoResolvers = typeInfoResolvers;
        _derivedTypesOptions = derivedTypesOptions;
    }

    public void Configure(JsonSerializerOptions options)
    {
        options.DefaultIgnoreCondition = JOptions.Base.DefaultIgnoreCondition;
        options.ReferenceHandler = JOptions.Base.ReferenceHandler;
        options.ReadCommentHandling = JOptions.Base.ReadCommentHandling;
        options.PropertyNameCaseInsensitive = JOptions.Base.PropertyNameCaseInsensitive;
        options.AllowTrailingCommas = JOptions.Base.AllowTrailingCommas;
        options.WriteIndented = JOptions.Base.WriteIndented;

        foreach (var resolver in _typeInfoResolvers)
        {
            options.TypeInfoResolverChain.Add(resolver);
        }

        options.TypeInfoResolverChain.Add(new PolymorphicJsonTypeInfoResolver(_derivedTypesOptions.Value));
        options.Converters.Add(DynamicJsonConverter.Instance);
        options.Converters.Add(PathStringJsonConverter.Instance);
    }
}
