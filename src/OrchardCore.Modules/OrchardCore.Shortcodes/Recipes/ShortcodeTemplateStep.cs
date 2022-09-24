using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

        public int Order => 0;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "ShortcodeTemplates", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.Step.Property("ShortcodeTemplates").Value is JObject templates)
            {
                foreach (var property in templates.Properties())
                {
                    var name = property.Name;
                    var value = property.Value.ToObject<ShortcodeTemplate>();

                    await _templatesManager.UpdateShortcodeTemplateAsync(name, value);
                }
            }
        }
    }
}
