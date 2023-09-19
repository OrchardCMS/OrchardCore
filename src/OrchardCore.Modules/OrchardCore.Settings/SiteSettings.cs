using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Documents;
using OrchardCore.Entities;

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

        public bool UseCdn { get; set; }

        public string CdnBaseUrl { get; set; }

        public RouteValueDictionary HomeRoute { get; set; } = new RouteValueDictionary();

        public bool AppendVersion { get; set; } = true;

        public CacheMode CacheMode { get; set; }

        public T As<T>() where T : new()
        {
            var name = typeof(T).Name;

            if (_cache.TryGetValue(name, out var obj) && obj is T value)
            {
                return value;
            }

            var settings = this.As<T>(name);

            _cache[name] = settings;

            return settings;
        }

        public ISite Put<T>(T settings) where T : new()
        {
            var name = typeof(T).Name;

            this.Put(name, settings);

            _cache.Remove(name);

            return this;
        }

        private readonly Dictionary<string, object> _cache = new();
    }
}
