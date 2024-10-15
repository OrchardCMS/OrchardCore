using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment;

public class SiteSettingsDeploymentSource
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

        var data = new JsonObject { ["name"] = "Settings" };

        foreach (var settingName in step.Settings)
        {
            switch (settingName)
            {
                case "BaseUrl":
                    data.Add(nameof(ISite.BaseUrl), site.BaseUrl);
                    break;

                case "Calendar":
                    data.Add(nameof(ISite.Calendar), site.Calendar);
                    break;

                case "MaxPagedCount":
                    data.Add(nameof(ISite.MaxPagedCount), site.MaxPagedCount);
                    break;

                case "MaxPageSize":
                    data.Add(nameof(ISite.MaxPageSize), site.MaxPageSize);
                    break;

                case "PageSize":
                    data.Add(nameof(ISite.PageSize), site.PageSize);
                    break;

                case "ResourceDebugMode":
                    data.Add(nameof(ISite.ResourceDebugMode), JsonValue.Create(site.ResourceDebugMode));
                    break;

                case "SiteName":
                    data.Add(nameof(ISite.SiteName), site.SiteName);
                    break;

                case "PageTitleFormat":
                    data.Add(nameof(ISite.PageTitleFormat), site.PageTitleFormat);
                    break;

                case "SiteSalt":
                    data.Add(nameof(ISite.SiteSalt), site.SiteSalt);
                    break;

                case "SuperUser":
                    data.Add(nameof(ISite.SuperUser), site.SuperUser);
                    break;

                case "TimeZoneId":
                    data.Add(nameof(ISite.TimeZoneId), site.TimeZoneId);
                    break;

                case "UseCdn":
                    data.Add(nameof(ISite.UseCdn), site.UseCdn);
                    break;

                case "CdnBaseUrl":
                    data.Add(nameof(ISite.CdnBaseUrl), site.CdnBaseUrl);
                    break;

                case "AppendVersion":
                    data.Add(nameof(ISite.AppendVersion), site.AppendVersion);
                    break;

                case "HomeRoute":
                    data.Add(nameof(ISite.HomeRoute), JObject.FromObject(site.HomeRoute));
                    break;

                case "CacheMode":
                    data.Add(nameof(ISite.CacheMode), JsonValue.Create(site.CacheMode));
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported setting '{settingName}'");
            }
        }

        result.Steps.Add(data);
    }
}
