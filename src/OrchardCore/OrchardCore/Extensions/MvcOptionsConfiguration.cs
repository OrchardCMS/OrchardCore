using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Json;

namespace OrchardCore.Extensions;

internal sealed class MvcOptionsConfiguration : IConfigureOptions<MvcOptions>
{
    private readonly DocumentJsonSerializerOptions _documentOptions;

    public MvcOptionsConfiguration(IOptions<DocumentJsonSerializerOptions> documentOptions)
    {
        _documentOptions = documentOptions.Value;
    }

    public void Configure(MvcOptions options)
    {
        options.OutputFormatters.Insert(0, new DocumentSystemTextJsonOutputFormatter(_documentOptions.SerializerOptions));
    }
}
