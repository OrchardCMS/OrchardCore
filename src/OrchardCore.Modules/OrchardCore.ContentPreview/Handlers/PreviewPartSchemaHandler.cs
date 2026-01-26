using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentPreview.Handlers;

internal sealed class PreviewPartSchemaHandler : IContentPartSchemaHandler
{
    public JsonSchema GetSettingsSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("PreviewPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();
    }
}
