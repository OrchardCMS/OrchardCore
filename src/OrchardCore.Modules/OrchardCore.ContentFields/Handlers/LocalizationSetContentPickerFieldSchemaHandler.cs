using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class LocalizationSetContentPickerFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("LocalizationSetContentPickerFieldSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Hint", new RecipeStepSchemaBuilder().TypeString()),
                        ("Required", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("Multiple", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("DisplayedContentTypes", new RecipeStepSchemaBuilder().TypeArray()))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
