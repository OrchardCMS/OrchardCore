using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;

namespace OrchardCore.Settings
{
    // When updating class also update SiteSettingsDeploymentSource and SettingsStep.
    public class SiteSettings : Entity, ISite
    {
        public int Id { get; set; }
        public string BaseUrl { get; set; }
        public string Calendar { get; set; }
        public int MaxPagedCount { get; set; }
        public int MaxPageSize { get; set; }
        public int PageSize { get; set; }
        public string TimeZoneId { get; set; }
        public ResourceDebugMode ResourceDebugMode { get; set; }
        public string SiteName { get; set; }
        public string SiteSalt { get; set; }
        public string PageTitleFormat { get; set; }
        public string SuperUser { get; set; }
        public bool UseCdn { get; set; } = true;
        public string CdnBaseUrl { get; set; }
        public RouteValueDictionary HomeRoute { get; set; } = new RouteValueDictionary();
        public bool AppendVersion { get; set; } = true;

        public SiteSettings UpdateFrom(ISite site)
        {
            BaseUrl = site.BaseUrl;
            Calendar = site.Calendar;
            MaxPagedCount = site.MaxPagedCount;
            MaxPageSize = site.MaxPageSize;
            PageSize = site.PageSize;
            TimeZoneId = site.TimeZoneId;
            ResourceDebugMode = site.ResourceDebugMode;
            SiteName = site.SiteName;
            SiteSalt = site.SiteSalt;
            PageTitleFormat = site.PageTitleFormat;
            SuperUser = site.SuperUser;
            UseCdn = site.UseCdn;
            CdnBaseUrl = site.CdnBaseUrl;
            AppendVersion = site.AppendVersion;

            Properties = new JObject(site.Properties);
            HomeRoute = new RouteValueDictionary(site.HomeRoute);

            return this;
        }

        public SiteSettings Clone()
        {
            return new SiteSettings() { Id = Id }.UpdateFrom(this);
        }
    }
}
