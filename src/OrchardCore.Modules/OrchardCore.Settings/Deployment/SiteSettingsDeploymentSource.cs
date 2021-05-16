using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _siteService;

        public SiteSettingsDeploymentSource(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var settingsStep = step as SiteSettingsDeploymentStep;
            if (settingsStep == null)
            {
                return;
            }

            var site = await _siteService.GetSiteSettingsAsync();

            var data = new JObject(new JProperty("name", "Settings"));

            foreach (var settingName in settingsStep.Settings)
            {
                switch (settingName)
                {
                    case "BaseUrl":
                        data.Add(new JProperty(nameof(ISite.BaseUrl), site.BaseUrl));
                        break;

                    case "Calendar":
                        data.Add(new JProperty(nameof(ISite.Calendar), site.Calendar));
                        break;

                    case "MaxPagedCount":
                        data.Add(new JProperty(nameof(ISite.MaxPagedCount), site.MaxPagedCount));
                        break;

                    case "MaxPageSize":
                        data.Add(new JProperty(nameof(ISite.MaxPageSize), site.MaxPageSize));
                        break;

                    case "PageSize":
                        data.Add(new JProperty(nameof(ISite.PageSize), site.PageSize));
                        break;

                    case "ResourceDebugMode":
                        data.Add(new JProperty(nameof(ISite.ResourceDebugMode), site.ResourceDebugMode));
                        break;

                    case "SiteName":
                        data.Add(new JProperty(nameof(ISite.SiteName), site.SiteName));
                        break;

                    case "PageTitleFormat":
                        data.Add(new JProperty(nameof(ISite.PageTitleFormat), site.PageTitleFormat));
                        break;

                    case "SiteSalt":
                        data.Add(new JProperty(nameof(ISite.SiteSalt), site.SiteSalt));
                        break;

                    case "SuperUser":
                        data.Add(new JProperty(nameof(ISite.SuperUser), site.SuperUser));
                        break;

                    case "TimeZoneId":
                        data.Add(new JProperty(nameof(ISite.TimeZoneId), site.TimeZoneId));
                        break;

                    case "UseCdn":
                        data.Add(new JProperty(nameof(ISite.UseCdn), site.UseCdn));
                        break;

                    case "CdnBaseUrl":
                        data.Add(new JProperty(nameof(ISite.CdnBaseUrl), site.CdnBaseUrl));
                        break;

                    case "AppendVersion":
                        data.Add(new JProperty(nameof(ISite.AppendVersion), site.AppendVersion));
                        break;

                    case "HomeRoute":
                        data.Add(new JProperty(nameof(ISite.HomeRoute), JObject.FromObject(site.HomeRoute)));
                        break;

                    case "CacheMode":
                        data.Add(new JProperty(nameof(ISite.CacheMode), site.CacheMode));
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported setting '{settingName}'");
                }
            }

            result.Steps.Add(data);

            return;
        }
    }
}
