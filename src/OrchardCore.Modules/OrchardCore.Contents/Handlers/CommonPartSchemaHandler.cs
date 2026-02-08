using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Contents.Handlers;

internal sealed class CommonPartSchemaHandler : IContentPartSchemaHandler
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
                ("CommonPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("OwnerEditor", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("DateEditor", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("DisplayDateEditor", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("DisplayOwnerEditor", new RecipeStepSchemaBuilder().TypeBoolean()))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
