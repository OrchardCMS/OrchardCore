using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Recipes;

/// <summary>
/// This recipe step creates a set of Shortcodes.
/// </summary>
public sealed class ShortcodeTemplateStep : NamedRecipeStepHandler
{
    private readonly ShortcodeTemplatesManager _templatesManager;

    public ShortcodeTemplateStep(ShortcodeTemplatesManager templatesManager)
        : base("ShortcodeTemplates")
    {
        _templatesManager = templatesManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
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
