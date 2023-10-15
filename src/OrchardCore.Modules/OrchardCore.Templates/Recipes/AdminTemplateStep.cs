using System.Threading.Tasks;
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

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (context.TryGetStepPropertyIfNameMatches<Template>("AdminTemplates", out var templates))
            {
                foreach (var (name, value) in templates)
                {
                    await _adminTemplatesManager.UpdateTemplateAsync(name, value);
                }
            }
        }
    }
}
