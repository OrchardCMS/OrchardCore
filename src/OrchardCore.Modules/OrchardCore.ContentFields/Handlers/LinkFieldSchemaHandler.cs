using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class LinkFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("LinkFieldSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Hint", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Required", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("HintLinkText", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("LinkTextMode", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("UrlPlaceholder", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("TextPlaceholder", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("DefaultUrl", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("DefaultText", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("DefaultTarget", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
