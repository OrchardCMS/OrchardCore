using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes;

public sealed class TemplateRecipeStep : RecipeImportStep<object>
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

    protected override async Task ImportAsync(object model, RecipeExecutionContext context)
    {
        if (context.Step.TryGetPropertyValue("Templates", out var jsonNode) && jsonNode is JsonObject templates)
        {
            foreach (var property in templates)
            {
                var name = property.Key;
                var value = property.Value.ToObject<Template>();

                await _templatesManager.UpdateTemplateAsync(name, value);
            }
        }
    }
}
