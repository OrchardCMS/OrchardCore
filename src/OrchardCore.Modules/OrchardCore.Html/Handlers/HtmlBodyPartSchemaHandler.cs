using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Html.Handlers;

internal sealed class HtmlBodyPartSchemaHandler : IContentPartSchemaHandler
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
                ("HtmlBodyPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("SanitizeHtml", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("HtmlEditor", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("InsertMediaWithUrl", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DefaultPosition", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
