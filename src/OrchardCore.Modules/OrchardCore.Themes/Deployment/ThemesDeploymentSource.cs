using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.Themes.Recipes;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Deployment
{
    public class ThemesDeploymentSource : IDeploymentSource
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IAdminThemeService _adminThemeService;

        public ThemesDeploymentSource(ISiteThemeService siteThemeService, IAdminThemeService adminThemeService)
        {
            _siteThemeService = siteThemeService;
            _adminThemeService = adminThemeService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var themesStep = step as ThemesDeploymentStep;

            if (themesStep == null)
            {
                return;
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "Themes"),
                new JProperty(nameof(ThemeStepModel.Site), await _siteThemeService.GetSiteThemeNameAsync()),
                new JProperty(nameof(ThemeStepModel.Admin), await _adminThemeService.GetAdminThemeNameAsync())
            ));
        }
    }
}
