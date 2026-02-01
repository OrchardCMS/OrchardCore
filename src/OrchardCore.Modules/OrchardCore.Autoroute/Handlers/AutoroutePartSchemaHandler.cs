using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Autoroute.Handlers;

internal sealed class AutoroutePartSchemaHandler : IContentPartSchemaHandler
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
                ("AutoroutePartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("AllowCustomPath", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("AllowUpdatePath", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("ShowHomepageOption", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("AllowDisabled", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("Pattern", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("DefaultPatternIndex", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
                        ("PatternIndex", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
                        ("PerItemConfiguration", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("RouteContainedItems", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("RenderTitle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("UpdateRouteWithChanges", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("ManageContainedItemRoutes", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("Absolute", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("ShowTitleInLink", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("AllowEmptyPath", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                        ("AllowConflict", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
