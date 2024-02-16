using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ShortcodeTemplateStep(
            ShortcodeTemplatesManager templatesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _templatesManager = templatesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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
                    var value = property.Value.ToObject<ShortcodeTemplate>(_jsonSerializerOptions);

                    await _templatesManager.UpdateShortcodeTemplateAsync(name, value);
                }
            }
        }
    }
}
