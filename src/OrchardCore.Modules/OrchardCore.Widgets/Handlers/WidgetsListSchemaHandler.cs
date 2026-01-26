using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Widgets.Handlers;

internal sealed class WidgetsListSchemaHandler : IContentPartSchemaHandler
{
    private JsonSchema _schema;

    public JsonSchema GetSettingsSchema()
    {
        if (_schema != null)
        {
            return _schema;
        }

        var builder = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("WidgetsListPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Zones", new JsonSchemaBuilder()
                            .Type(SchemaValueType.Array)
                            .Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
                        )
                    )
                    .AdditionalProperties(false) // only allow defined keys inside WidgetsListPartSettings
                )
            )
            .AdditionalProperties(true); // allow other part-level settings alongside

        _schema = builder.Build();

        return _schema;
    }
}
