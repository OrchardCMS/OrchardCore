using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentPreview.Handlers;

internal sealed class PreviewPartSchemaHandler : IContentPartSchemaHandler
{
    public JsonSchema GetSettingsSchema()
    {
        return new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("PreviewPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();
    }
}
