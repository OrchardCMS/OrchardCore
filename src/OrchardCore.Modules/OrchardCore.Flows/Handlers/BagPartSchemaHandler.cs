using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Flows.Handlers;

internal sealed class BagPartSchemaHandler : IContentPartSchemaHandler
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
                ("BagPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("ContainedContentTypes", new RecipeStepSchemaBuilder()
                            .TypeArray()
                            .Items(new RecipeStepSchemaBuilder().TypeString())),
                        ("DisplayType", new RecipeStepSchemaBuilder().TypeString()),
                        ("Count", new RecipeStepSchemaBuilder().TypeInteger()),
                        ("ShowHeader", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("ReadOnly", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("OwnerEditor", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("InlineEditing", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("Many", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("CreateContentType", new RecipeStepSchemaBuilder().TypeString()),
                        ("EnableItemReordering", new RecipeStepSchemaBuilder().TypeBoolean()))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
