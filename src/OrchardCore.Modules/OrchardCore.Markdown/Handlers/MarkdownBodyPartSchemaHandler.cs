using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Markdown.Handlers;

internal sealed class MarkdownBodyPartSchemaHandler : IContentPartSchemaHandler
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
                ("MarkdownBodyPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("MarkdownEditor", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("SanitizeHtml", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DefaultPosition", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
