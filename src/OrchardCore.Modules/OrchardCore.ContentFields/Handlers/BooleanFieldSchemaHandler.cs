using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class BooleanFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("BooleanFieldSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Label", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The label for the boolean field")
                        ),
                        ("DefaultValue", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Description("The default value for this field")
                        )
                    )
                    .AdditionalProperties(false) // only those 3 keys are allowed inside
                )
            )
            .AdditionalProperties(true); // allow other part-level settings alongside

        _schema = builder.Build();

        return _schema;
    }
}
