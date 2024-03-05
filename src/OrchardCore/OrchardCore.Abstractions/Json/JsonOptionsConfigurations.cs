using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace OrchardCore.Json;

public class JsonOptionsConfigurations : IConfigureOptions<JsonOptions>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonOptionsConfigurations(IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    public void Configure(JsonOptions options)
    {
        options.SerializerOptions.DefaultIgnoreCondition = _jsonSerializerOptions.DefaultIgnoreCondition;
        options.SerializerOptions.ReferenceHandler = _jsonSerializerOptions.ReferenceHandler;
        options.SerializerOptions.ReadCommentHandling = _jsonSerializerOptions.ReadCommentHandling;
        options.SerializerOptions.PropertyNameCaseInsensitive = _jsonSerializerOptions.PropertyNameCaseInsensitive;
        options.SerializerOptions.AllowTrailingCommas = _jsonSerializerOptions.AllowTrailingCommas;
        options.SerializerOptions.WriteIndented = _jsonSerializerOptions.WriteIndented;
        options.SerializerOptions.PropertyNamingPolicy = _jsonSerializerOptions.PropertyNamingPolicy;
        options.SerializerOptions.Encoder = _jsonSerializerOptions.Encoder;
        options.SerializerOptions.TypeInfoResolver = _jsonSerializerOptions.TypeInfoResolver;

        foreach (var resolver in _jsonSerializerOptions.TypeInfoResolverChain)
        {
            options.SerializerOptions.TypeInfoResolverChain.Add(resolver);
        }

        foreach (var converter in _jsonSerializerOptions.Converters)
        {
            options.SerializerOptions.Converters.Add(converter);
        }
    }
}
