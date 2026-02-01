using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class LocalizationSetContentPickerFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("LocalizationSetContentPickerFieldSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Hint", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Required", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("Multiple", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DisplayedContentTypes", new JsonSchemaBuilder().Type(SchemaValueType.Array)))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
