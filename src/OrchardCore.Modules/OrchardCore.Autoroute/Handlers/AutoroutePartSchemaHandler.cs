using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Autoroute.Handlers;

internal sealed class AutoroutePartSchemaHandler : IContentPartSchemaHandler
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
                ("AutoroutePartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("AllowCustomPath", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("AllowUpdatePath", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("ShowHomepageOption", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("AllowDisabled", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("Pattern", new RecipeStepSchemaBuilder().TypeString()),
                        ("DefaultPatternIndex", new RecipeStepSchemaBuilder().TypeInteger()),
                        ("PatternIndex", new RecipeStepSchemaBuilder().TypeInteger()),
                        ("PerItemConfiguration", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("RouteContainedItems", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("RenderTitle", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("UpdateRouteWithChanges", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("ManageContainedItemRoutes", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("Absolute", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("ShowTitleInLink", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("AllowEmptyPath", new RecipeStepSchemaBuilder().TypeBoolean()),
                        ("AllowConflict", new RecipeStepSchemaBuilder().TypeBoolean()))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
