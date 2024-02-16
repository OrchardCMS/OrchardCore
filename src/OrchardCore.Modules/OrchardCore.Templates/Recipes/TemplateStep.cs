using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes
{
    /// <summary>
    /// This recipe step creates a set of templates.
    /// </summary>
    public class TemplateStep : IRecipeStepHandler
    {
        private readonly TemplatesManager _templatesManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public TemplateStep(
            TemplatesManager templatesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _templatesManager = templatesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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
                    var value = property.Value.ToObject<Template>(_jsonSerializerOptions);

                    await _templatesManager.UpdateTemplateAsync(name, value);
                }
            }
        }
    }
}
