using System.Text.Json;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;
using Microsoft.Extensions.Options;
using OrchardCore.Json.Extensions;

namespace OrchardCore.Json;

internal sealed class JsonOptionsConfigurations: IConfigureOptions<HttpJsonOptions>, IConfigureOptions<MvcJsonOptions>

{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonOptionsConfigurations(IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    public void Configure(HttpJsonOptions options)
        => options.SerializerOptions.Merge(_jsonSerializerOptions);

    public void Configure(MvcJsonOptions options)
        => options.JsonSerializerOptions.Merge(_jsonSerializerOptions);
}
