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
    public class TemplateStep : IRecipeStepHandler
    {
        private readonly TemplatesManager _templatesManager;

        public TemplateStep(TemplatesManager templatesManager)
        {
            _templatesManager = templatesManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Templates", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.Step.Property("Templates").Value is JObject templates)
            {
                foreach (var property in templates.Properties())
                {
                    var name = property.Name;
                    var value = property.Value.ToObject<Template>();

                    await _templatesManager.UpdateTemplateAsync(name, value);
                }
            }
        }
    }
}
