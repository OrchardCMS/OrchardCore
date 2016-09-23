using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Admin;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Themes.Services;

namespace Orchard.Themes.Recipes
{
    public class ThemesStep : RecipeExecutionStep
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IAdminThemeService _adminThemeService;

        public ThemesStep(
            ISiteThemeService siteThemeService,
            IAdminThemeService adminThemeService,
            ILoggerFactory logger,
            IStringLocalizer<ThemesStep> localizer) : base(logger, localizer)
        {
            _adminThemeService = adminThemeService;
            _siteThemeService = siteThemeService;
        }

        public override string Name
        {
            get { return "Themes"; }
        }

        public override async Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var model = recipeContext.RecipeStep.Step.ToObject<ThemeStepModel>();

            if (!String.IsNullOrEmpty(model.Site))
            {
                await _siteThemeService.SetSiteThemeAsync(model.Site);
            }

            if (!String.IsNullOrEmpty(model.Admin))
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