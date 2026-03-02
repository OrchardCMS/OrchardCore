using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class LinkFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("LinkFieldSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Hint", new RecipeStepSchemaBuilder().TypeString()),
                        ("Required", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("HintLinkText", new RecipeStepSchemaBuilder().TypeString()),
                        ("LinkTextMode", new RecipeStepSchemaBuilder().TypeString()),
                        ("UrlPlaceholder", new RecipeStepSchemaBuilder().TypeString()),
                        ("TextPlaceholder", new RecipeStepSchemaBuilder().TypeString()),
                        ("DefaultUrl", new RecipeStepSchemaBuilder().TypeString()),
                        ("DefaultText", new RecipeStepSchemaBuilder().TypeString()),
                        ("DefaultTarget", new RecipeStepSchemaBuilder().TypeString()))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
