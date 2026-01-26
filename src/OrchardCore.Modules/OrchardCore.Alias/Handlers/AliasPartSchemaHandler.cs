using Json.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Alias.Handlers;

internal sealed class AliasPartSchemaHandler : IContentPartSchemaHandler
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
                ("AliasPartSettings", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Object)
                    .Properties(
                        ("Pattern", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Default("{{ Model.ContentItem.DisplayText | slugify }}")
                            .Description("The pattern used to generate the alias. Must be valid Liquid syntax.")),
                        ("Options", new JsonSchemaBuilder()
                            .Type(SchemaValueType.String)
                            .Enum("Editable", "GeneratedDisabled")
                            .Default("Editable")
                            .Description("Defines whether the alias is editable or generated-disabled.")))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
