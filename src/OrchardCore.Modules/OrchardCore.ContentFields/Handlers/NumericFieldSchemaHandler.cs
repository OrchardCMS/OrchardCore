using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class NumericFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("NumericFieldSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Hint", new RecipeStepSchemaBuilder().TypeString()),
                        ("Required", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("Scale", new RecipeStepSchemaBuilder().TypeInteger()),
                        ("Minimum", new RecipeStepSchemaBuilder().TypeNumber()),
                        ("Maximum", new RecipeStepSchemaBuilder().TypeNumber()),
                        ("Placeholder", new RecipeStepSchemaBuilder().TypeString()),
                        ("DefaultValue", new RecipeStepSchemaBuilder().TypeString()))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
