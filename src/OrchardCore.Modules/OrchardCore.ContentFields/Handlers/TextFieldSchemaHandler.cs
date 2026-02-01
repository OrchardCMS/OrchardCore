using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class TextFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("TextFieldSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Hint", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Required", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DefaultValue", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Type", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Pattern", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Placeholder", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
