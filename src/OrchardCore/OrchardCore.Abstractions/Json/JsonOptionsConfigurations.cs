using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using OrchardCore.Json.Extensions;

namespace OrchardCore.Json;

internal sealed class JsonOptionsConfigurations : IConfigureOptions<JsonOptions>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonOptionsConfigurations(IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    public void Configure(JsonOptions options)
    {
        options.SerializerOptions.Merge(_jsonSerializerOptions);
    }
}
