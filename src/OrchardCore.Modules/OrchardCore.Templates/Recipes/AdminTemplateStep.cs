using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

        public AdminTemplateStep(AdminTemplatesManager templatesManager)
        {
            _adminTemplatesManager = templatesManager;
        }

        public int Order => 0;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "AdminTemplates", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.Step.Property("AdminTemplates").Value is JObject templates)
            {
                foreach (var property in templates.Properties())
                {
                    var name = property.Name;
                    var value = property.Value.ToObject<Template>();

                    await _adminTemplatesManager.UpdateTemplateAsync(name, value);
                }
            }
        }
    }
}
