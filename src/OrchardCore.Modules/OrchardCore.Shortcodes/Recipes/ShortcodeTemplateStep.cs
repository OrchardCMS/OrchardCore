using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Recipes
{
    /// <summary>
    /// This recipe step creates a set of shortcodes.
    /// </summary>
    public class ShortcodeTemplateStep : IRecipeStepHandler
    {
        private readonly ShortcodeTemplatesManager _templatesManager;

        public ShortcodeTemplateStep(ShortcodeTemplatesManager templatesManager)
        {
            _templatesManager = templatesManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (context.TryGetStepPropertyIfNameMatches<ShortcodeTemplate>("ShortcodeTemplates", out var templates))
            {
                foreach (var (name, value) in templates)
                {
                    await _templatesManager.UpdateShortcodeTemplateAsync(name, value);
                }
            }
        }
    }
}
