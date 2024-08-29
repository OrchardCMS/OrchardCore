using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes;

/// <summary>
/// This recipe step creates a set of templates.
/// </summary>
public sealed class AdminTemplateStep : IRecipeStepHandler
{
    private readonly AdminTemplatesManager _adminTemplatesManager;

    public AdminTemplateStep(AdminTemplatesManager templatesManager)
    {
        _adminTemplatesManager = templatesManager;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "AdminTemplates", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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
