using OrchardCore.Recipes.Schema;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Flows.Handlers;

internal sealed class FlowPartSchemaHandler : IContentPartSchemaHandler
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
                ("FlowPartSettings", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("ContainedContentTypes", new RecipeStepSchemaBuilder()
                            .TypeArray()
                            .Items(new RecipeStepSchemaBuilder().TypeString())),
                        ("DisplayType", new RecipeStepSchemaBuilder().TypeString()),
                        ("FlowPart", new RecipeStepSchemaBuilder().TypeBoolean()))
                    .AdditionalProperties(true)))
            .AdditionalProperties(true)
            .Build();

        return _schema;
    }
}
