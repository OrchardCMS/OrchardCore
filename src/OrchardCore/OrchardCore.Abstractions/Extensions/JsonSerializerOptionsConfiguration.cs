using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Extensions;

public class JsonSerializerOptionsConfiguration : IConfigureOptions<JsonSerializerOptions>
{
    private readonly JsonDerivedTypesOptions _derivedTypesOptions;

    public JsonSerializerOptionsConfiguration(
        IOptions<JsonDerivedTypesOptions> derivedTypesOptions)
    {
        _derivedTypesOptions = derivedTypesOptions.Value;
    }

    public void Configure(JsonSerializerOptions options)
    {
        options.DefaultIgnoreCondition = JOptions.Base.DefaultIgnoreCondition;
        options.ReferenceHandler = JOptions.Base.ReferenceHandler;
        options.ReadCommentHandling = JOptions.Base.ReadCommentHandling;
        options.PropertyNameCaseInsensitive = JOptions.Base.PropertyNameCaseInsensitive;
        options.AllowTrailingCommas = JOptions.Base.AllowTrailingCommas;
        options.WriteIndented = JOptions.Base.WriteIndented;

        options.TypeInfoResolverChain.Add(new PolymorphicJsonTypeInfoResolver(_derivedTypesOptions));
        options.Converters.Add(DynamicJsonConverter.Instance);
        options.Converters.Add(PathStringJsonConverter.Instance);
    }
}
