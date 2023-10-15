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
            if (context.TryGetStepPropertyIfNameMatches<Template>("Templates", out var templates))
            {
                foreach (var (name, value) in templates)
                {
                    await _templatesManager.UpdateTemplateAsync(name, value);
                }
            }
        }
    }
}
