using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Recipes;

public sealed class ShortcodeTemplateRecipeStep : RecipeDeploymentStep<ShortcodeTemplateRecipeStep.ShortcodeTemplatesStepModel>
{
    private readonly ShortcodeTemplatesManager _templatesManager;

    public ShortcodeTemplateRecipeStep(ShortcodeTemplatesManager templatesManager)
    {
        _templatesManager = templatesManager;
    }

    public override string Name => "ShortcodeTemplates";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Shortcode Templates")
            .Description("Creates or updates shortcode templates.")
            .Required("name", "ShortcodeTemplates")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ShortcodeTemplates", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .AdditionalProperties(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Properties(
                            ("Content", new RecipeStepSchemaBuilder().TypeString()),
                            ("Hint", new RecipeStepSchemaBuilder().TypeString()),
                            ("Usage", new RecipeStepSchemaBuilder().TypeString()),
                            ("DefaultValue", new RecipeStepSchemaBuilder().TypeString()),
                            ("Categories", new RecipeStepSchemaBuilder()
                                .TypeArray()
                                .Items(new RecipeStepSchemaBuilder().TypeString())))
                        .AdditionalProperties(true)
                        .Build())
                    .Description("A dictionary keyed by shortcode name.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(ShortcodeTemplatesStepModel model, RecipeExecutionContext context)
    {
        foreach (var template in model.ShortcodeTemplates)
        {
            await _templatesManager.UpdateShortcodeTemplateAsync(template.Key, template.Value);
        }
    }

    protected override async Task<ShortcodeTemplatesStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var templates = await _templatesManager.GetShortcodeTemplatesDocumentAsync();

        return new ShortcodeTemplatesStepModel
        {
            ShortcodeTemplates = templates.ShortcodeTemplates.ToDictionary(k => k.Key, v => v.Value),
        };
    }

    public sealed class ShortcodeTemplatesStepModel
    {
        public Dictionary<string, ShortcodeTemplate> ShortcodeTemplates { get; set; }
    }
}
