using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Flows.Handlers;

internal sealed class BagPartSchemaHandler : IContentPartSchemaHandler
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
                ("BagPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("ContainedContentTypes", new JsonSchemaBuilder()
                            .Type(SchemaValueType.Array)
                            .Items(new JsonSchemaBuilder().Type(SchemaValueType.String))),
                        ("DisplayType", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Count", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
                        ("ShowHeader", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("ReadOnly", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("OwnerEditor", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("InlineEditing", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("Many", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("CreateContentType", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("EnableItemReordering", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
