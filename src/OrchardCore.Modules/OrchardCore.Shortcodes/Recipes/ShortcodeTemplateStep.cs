using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Recipes;

/// <summary>
/// This recipe step creates a set of shortcodes.
/// </summary>
public sealed class ShortcodeTemplateStep : IRecipeStepHandler
{
    private readonly ShortcodeTemplatesManager _templatesManager;

    public ShortcodeTemplateStep(ShortcodeTemplatesManager templatesManager)
    {
        _templatesManager = templatesManager;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "ShortcodeTemplates", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (context.Step.TryGetPropertyValue("ShortcodeTemplates", out var jsonNode) && jsonNode is JsonObject templates)
        {
            foreach (var property in templates)
            {
                var name = property.Key;
                var value = property.Value.ToObject<ShortcodeTemplate>();

                await _templatesManager.UpdateShortcodeTemplateAsync(name, value);
            }
        }
    }
}
