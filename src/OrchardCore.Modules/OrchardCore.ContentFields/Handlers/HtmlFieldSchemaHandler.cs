using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class HtmlFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("HtmlFieldSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Hint", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Required", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("SanitizeHtml", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
