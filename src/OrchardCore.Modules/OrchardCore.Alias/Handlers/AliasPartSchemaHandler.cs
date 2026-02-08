using OrchardCore.Recipes.Schema;
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

        _schema = new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("AliasPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Pattern", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Default("{{ Model.ContentItem.DisplayText | slugify }}")
                            .Description("The pattern used to generate the alias. Must be valid Liquid syntax.")),
                        ("Options", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Enum("Editable", "GeneratedDisabled")
                            .Default("Editable")
                            .Description("Defines whether the alias is editable or generated-disabled.")))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
