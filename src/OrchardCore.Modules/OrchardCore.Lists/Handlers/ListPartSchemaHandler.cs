using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Lists.Handlers;

internal sealed class ListPartSchemaHandler : IContentPartSchemaHandler
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
                ("ListPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("ContainedContentTypes", new JsonSchemaBuilder()
                            .Type(SchemaValueType.Array)
                            .Items(new JsonSchemaBuilder().Type(SchemaValueType.String))),
                        ("EnableOrdering", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("PageSize", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
                        ("ShowHeader", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("DisplayType", new JsonSchemaBuilder().Type(SchemaValueType.String)))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
