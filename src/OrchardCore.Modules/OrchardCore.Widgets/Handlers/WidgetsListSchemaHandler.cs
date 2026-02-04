using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Widgets.Handlers;

internal sealed class WidgetsListSchemaHandler : IContentPartSchemaHandler
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
                ("WidgetsListPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("Zones", new RecipeStepSchemaBuilder()
                            .TypeArray()
                            .Items(new RecipeStepSchemaBuilder().TypeString())
                        )
                    )
                    .AdditionalProperties(false) // only allow defined keys inside WidgetsListPartSettings
                )
            )
            .AdditionalProperties(true); // allow other part-level settings alongside

        _schema = builder.Build();

        return _schema;
    }
}
