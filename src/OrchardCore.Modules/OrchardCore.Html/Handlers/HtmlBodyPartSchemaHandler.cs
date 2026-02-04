using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Html.Handlers;

internal sealed class HtmlBodyPartSchemaHandler : IContentPartSchemaHandler
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
                ("HtmlBodyPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("SanitizeHtml", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("HtmlEditor", new RecipeStepSchemaBuilder().TypeString()),
                        ("InsertMediaWithUrl", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("DefaultPosition", new RecipeStepSchemaBuilder().TypeString()))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
