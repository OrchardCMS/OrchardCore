using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Recipes
{
    /// <summary>
    /// This recipe step defines the site and admin current themes.
    /// </summary>
    public class ThemesStep : IRecipeStepHandler
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IAdminThemeService _adminThemeService;

        public ThemesStep(
            ISiteThemeService siteThemeService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IAdminThemeService adminThemeService)
        {
            _adminThemeService = adminThemeService;
            _siteThemeService = siteThemeService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "Themes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<ThemeStepModel>(_jsonSerializerOptions);

            if (!string.IsNullOrEmpty(model.Site))
            {
                await _siteThemeService.SetSiteThemeAsync(model.Site);
            }

            if (!string.IsNullOrEmpty(model.Admin))
            {
                await _adminThemeService.SetAdminThemeAsync(model.Admin);
            }
        }
    }

    public class ThemeStepModel
    {
        public string Site { get; set; }
        public string Admin { get; set; }
    }
}
