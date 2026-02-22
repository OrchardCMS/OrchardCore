using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class DateTimeFieldSchemaHandler : IContentFieldSchemaHandler
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
                ("DateTimeFieldSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Hint", new RecipeStepSchemaBuilder().TypeString()),
                        ("Required", new RecipeStepSchemaBuilder().TypeBoolean()))
                    .AdditionalProperties(false)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
