using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Html.Handlers;

internal sealed class HtmlBodyPartSchemaHandler : IContentPartSchemaHandler
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
