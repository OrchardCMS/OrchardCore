using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes;

public sealed class AdminTemplateRecipeStep : RecipeImportStep<object>
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

    protected override async Task ImportAsync(object model, RecipeExecutionContext context)
    {
        if (context.Step.TryGetPropertyValue("AdminTemplates", out var jsonNode) && jsonNode is JsonObject templates)
        {
            foreach (var property in templates)
            {
                var name = property.Key;
                var value = property.Value.ToObject<Template>();

                await _adminTemplatesManager.UpdateTemplateAsync(name, value);
            }
        }
    }
}
