using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes;

/// <summary>
/// This recipe step creates a set of templates.
/// </summary>
public sealed class TemplateStep : IRecipeStepHandler
{
    private readonly TemplatesManager _templatesManager;

    public TemplateStep(
        TemplatesManager templatesManager)
    {
        _templatesManager = templatesManager;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "Templates", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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
