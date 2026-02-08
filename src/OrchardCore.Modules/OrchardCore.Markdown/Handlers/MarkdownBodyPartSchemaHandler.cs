using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Markdown.Handlers;

internal sealed class MarkdownBodyPartSchemaHandler : IContentPartSchemaHandler
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
                ("MarkdownBodyPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("MarkdownEditor", new RecipeStepSchemaBuilder().TypeString()),
                        ("SanitizeHtml", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("DefaultPosition", new RecipeStepSchemaBuilder().TypeString()))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
