using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment;

public sealed class SiteSettingsDeploymentSource
    : DeploymentSourceBase<SiteSettingsDeploymentStep>
{
    private readonly ISiteService _siteService;

    public SiteSettingsDeploymentSource(ISiteService siteService)
    {
        _siteService = siteService;
    }

    protected override async Task ProcessAsync(SiteSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var site = await _siteService.GetSiteSettingsAsync();

        result.Steps.Add(SiteSettingsDeploymentSelection.Export(site, step.Settings));
    }
}
