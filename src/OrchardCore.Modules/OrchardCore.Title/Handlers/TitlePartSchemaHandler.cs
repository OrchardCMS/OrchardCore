using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Title.Handlers;

internal sealed class TitlePartSchemaHandler : IContentPartSchemaHandler
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
                ("TitlePartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("RenderTitle", new RecipeStepSchemaBuilder()
                            .TypeBoolean()
                        ),
                        ("Options", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Enum("Editable", "GeneratedDisabled", "GeneratedHidden", "EditableRequired")
                            .Default("Editable")
                        ),
                        ("Pattern", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Description("This string must be a valid Liquid syntax")
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
