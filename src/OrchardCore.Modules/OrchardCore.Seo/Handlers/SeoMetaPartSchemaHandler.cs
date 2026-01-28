using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Seo.Handlers;

internal sealed class SeoMetaPartSchemaHandler : IContentPartSchemaHandler
{
    private JsonSchema _schema;

    public JsonSchema GetSettingsSchema()
    {
        if (_schema is not null)
        {
            return _schema;
        }

        _schema = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("SeoMetaPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("RenderTitle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("RenderKeywords", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("RenderDescription", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("RenderCustomMetaTags", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("MetaTags", new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(new JsonSchemaBuilder().Type(SchemaValueType.String))))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
