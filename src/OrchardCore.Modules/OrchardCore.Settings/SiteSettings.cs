using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using OrchardCore.Documents;

namespace OrchardCore.Settings
{
    // When updating class also update SiteSettingsDeploymentSource and SettingsStep.
    public class SiteSettings : DocumentEntity, ISite
    {
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

        [IgnoreMember]
        public RouteValueDictionary HomeRoute { get; set; } = new RouteValueDictionary();

        [JsonIgnore]
        public Dictionary<string, object> HomeRouteValues { get; set; }

        public bool AppendVersion { get; set; } = true;
        public string Meta { get; set; }

        public override void OnAfterDeserialize()
        {
            HomeRoute = new RouteValueDictionary(HomeRouteValues);
            base.OnAfterDeserialize();
        }

        public override void OnBeforeSerialize()
        {
            HomeRouteValues = HomeRoute.ToDictionary(kv => kv.Key, kv => kv.Value);
            base.OnBeforeSerialize();
        }
    }
}
