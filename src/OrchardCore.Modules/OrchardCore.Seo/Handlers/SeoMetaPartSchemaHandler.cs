using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Seo.Handlers;

internal sealed class SeoMetaPartSchemaHandler : IContentPartSchemaHandler
{
    private RecipeStepSchema _schema;

    public RecipeStepSchema GetSettingsSchema()
    {
        if (_schema is not null)
        {
            return _schema;
        }

        _schema = new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("SeoMetaPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("RenderTitle", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("RenderKeywords", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("RenderDescription", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("RenderCustomMetaTags", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("MetaTags", new RecipeStepSchemaBuilder().TypeArray().Items(new RecipeStepSchemaBuilder().TypeString())))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
