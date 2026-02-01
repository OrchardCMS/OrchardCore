using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Flows.Handlers;

internal sealed class FlowPartSchemaHandler : IContentPartSchemaHandler
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
                ("FlowPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("ContainedContentTypes", new JsonSchemaBuilder()
                            .Type(SchemaValueType.Array)
                            .Items(new JsonSchemaBuilder().Type(SchemaValueType.String))),
                        ("DisplayType", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("FlowPart", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
