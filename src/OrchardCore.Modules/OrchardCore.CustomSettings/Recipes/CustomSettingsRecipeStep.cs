using Json.Schema;
using OrchardCore.Contents.Core;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings.Recipes;

public sealed class CustomSettingsRecipeStep : RecipeImportStep<object>
{
    private readonly ISiteService _siteService;

    public CustomSettingsRecipeStep(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public override string Name => "custom-settings";

    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Title("Custom Settings")
            .Description("Imports custom site settings.")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const("custom-settings")
                    .Description("The name of the settings step.")))
            .Required("name")
            .MinProperties(2) // at least "name" + one content item
                              // Allow any other property to be a content item
            .AdditionalProperties(new JsonSchemaBuilder()
                .AllOf(ContentCommonSchemas.ContentItemSchema)
                .Build())
            .Build();
    }

    protected override async Task ImportAsync(object model, RecipeExecutionContext context)
    {
        var siteSettings = await _siteService.LoadSiteSettingsAsync();

        var step = context.Step;
        foreach (var customSettings in step)
        {
            if (customSettings.Key == "name")
            {
                continue;
            }

            siteSettings.Properties[customSettings.Key] = customSettings.Value.DeepClone();
        }

        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
