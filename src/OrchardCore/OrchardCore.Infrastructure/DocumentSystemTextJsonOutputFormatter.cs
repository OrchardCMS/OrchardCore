using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using OrchardCore.Data.Documents;
using OrchardCore.Entities;

namespace OrchardCore.Json;

public class DocumentSystemTextJsonOutputFormatter : SystemTextJsonOutputFormatter
{
    public DocumentSystemTextJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions)
        : base(jsonSerializerOptions)
    {
    }

    protected override bool CanWriteType(Type type)
    {
        return typeof(IDocument).IsAssignableFrom(type) ||
            typeof(IEntity).IsAssignableFrom(type);
    }
}
