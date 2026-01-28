using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Contents.Handlers;

internal sealed class CommonPartSchemaHandler : IContentPartSchemaHandler
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
                ("CommonPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("OwnerEditor", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DateEditor", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DisplayDateEditor", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DisplayOwnerEditor", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
