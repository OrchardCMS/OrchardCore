using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentFields.Handlers;

internal sealed class BooleanFieldSchemaHandler : IContentFieldSchemaHandler
{
    private RecipeStepSchema _schema;

    public RecipeStepSchema GetSettingsSchema()
    {
        if (_schema != null)
        {
            return _schema;
        }

        var builder = new RecipeStepSchemaBuilder()
            .TypeObject()
            .Properties(
                ("BooleanFieldSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Label", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The label for the boolean field")
                        ),
                        ("DefaultValue", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("The default value for this field")
                        )
                    )
                    .AdditionalProperties(false) // only those 3 keys are allowed inside
                )
            )
            .AdditionalProperties(true); // allow other part-level settings alongside

        _schema = builder.Build();

        return _schema;
    }
}
