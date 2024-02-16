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
    public class AdminTemplateStep : IRecipeStepHandler
    {
        private readonly AdminTemplatesManager _adminTemplatesManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AdminTemplateStep(
            AdminTemplatesManager templatesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _adminTemplatesManager = templatesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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
                    var value = property.Value.ToObject<Template>(_jsonSerializerOptions);

                    await _adminTemplatesManager.UpdateTemplateAsync(name, value);
                }
            }
        }
    }
}
