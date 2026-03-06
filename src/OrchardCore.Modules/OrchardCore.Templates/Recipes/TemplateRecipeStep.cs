using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes;

public sealed class TemplateRecipeStep : RecipeDeploymentStep<TemplateRecipeStep.TemplatesStepModel>
{
    private readonly TemplatesManager _templatesManager;

    public TemplateRecipeStep(TemplatesManager templatesManager)
    {
        _templatesManager = templatesManager;
    }

    public override string Name => "Templates";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Templates")
            .Description("Creates or updates Liquid templates.")
            .Required("name", "Templates")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Templates", new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .AdditionalProperties(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Properties(
                            ("Content", new RecipeStepSchemaBuilder().TypeString()),
                            ("Description", new RecipeStepSchemaBuilder().TypeString()))
                        .AdditionalProperties(true)
                        .Build())
                    .Description("A dictionary keyed by template name.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(TemplatesStepModel model, RecipeExecutionContext context)
    {
        if (model.Templates != null)
        {
            foreach (var template in model.Templates)
            {
                await _templatesManager.UpdateTemplateAsync(template.Key, template.Value);
            }
        }
    }

    protected override async Task<TemplatesStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var templates = await _templatesManager.GetTemplatesDocumentAsync();

        return new TemplatesStepModel
        {
            Templates = templates.Templates.ToDictionary(k => k.Key, v => v.Value),
        };
    }

    public sealed class TemplatesStepModel
    {
        public Dictionary<string, Template> Templates { get; set; }
    }
}
