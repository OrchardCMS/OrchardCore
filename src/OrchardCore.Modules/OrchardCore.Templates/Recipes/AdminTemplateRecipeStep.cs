using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes;

public sealed class AdminTemplateRecipeStep : RecipeDeploymentStep<AdminTemplateRecipeStep.AdminTemplatesStepModel>
{
    private readonly AdminTemplatesManager _adminTemplatesManager;

    public AdminTemplateRecipeStep(AdminTemplatesManager templatesManager)
    {
        _adminTemplatesManager = templatesManager;
    }

    public override string Name => "AdminTemplates";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Admin Templates")
            .Description("Creates or updates admin Liquid templates.")
            .Required("name", "AdminTemplates")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("AdminTemplates", new RecipeStepSchemaBuilder()
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

    protected override async Task ImportAsync(AdminTemplatesStepModel model, RecipeExecutionContext context)
    {
        if (model.AdminTemplates != null)
        {
            foreach (var template in model.AdminTemplates)
            {
                await _adminTemplatesManager.UpdateTemplateAsync(template.Key, template.Value);
            }
        }
    }

    protected override async Task<AdminTemplatesStepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var templates = await _adminTemplatesManager.GetTemplatesDocumentAsync();

        return new AdminTemplatesStepModel
        {
            AdminTemplates = templates.Templates.ToDictionary(k => k.Key, v => v.Value),
        };
    }

    public sealed class AdminTemplatesStepModel
    {
        public Dictionary<string, Template> AdminTemplates { get; set; }
    }
}
