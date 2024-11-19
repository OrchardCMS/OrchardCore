using System.Text.Json.Nodes;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Deployment;

public class ThemesDeploymentSource
    : DeploymentSourceBase<ThemesDeploymentStep>
{
    private readonly ISiteThemeService _siteThemeService;
    private readonly IAdminThemeService _adminThemeService;

    public ThemesDeploymentSource(ISiteThemeService siteThemeService, IAdminThemeService adminThemeService)
    {
        _siteThemeService = siteThemeService;
        _adminThemeService = adminThemeService;
    }

    protected override async Task ProcessAsync(ThemesDeploymentStep step, DeploymentPlanResult result)
    {
        result.Steps.Add(new JsonObject
        {
            ["name"] = "Themes",
            ["Site"] = await _siteThemeService.GetSiteThemeNameAsync(),
            ["Admin"] = await _adminThemeService.GetAdminThemeNameAsync(),
        });
    }
}
