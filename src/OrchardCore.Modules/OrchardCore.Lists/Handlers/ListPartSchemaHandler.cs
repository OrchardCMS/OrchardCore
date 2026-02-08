using OrchardCore.Recipes.Schema;
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

        _schema = new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("ListPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("ContainedContentTypes", new RecipeStepSchemaBuilder()
                            .TypeArray()
                            .Items(new RecipeStepSchemaBuilder().TypeString())),
                        ("EnableOrdering", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("PageSize", new RecipeStepSchemaBuilder().TypeInteger()),
                        ("ShowHeader", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("DisplayType", new RecipeStepSchemaBuilder().TypeString()))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
