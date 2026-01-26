using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class ContentPickerFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("ContentPickerFieldSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Hint", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Required", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("Multiple", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DisplayAllContentTypes", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DisplayedContentTypes", new JsonSchemaBuilder().Type(SchemaValueType.Array)),
                        ("DisplayedStereotypes", new JsonSchemaBuilder().Type(SchemaValueType.Array)),
                        ("Placeholder", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("TitlePattern", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
