using System.Collections.Concurrent;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Documents;
using OrchardCore.Entities;

namespace OrchardCore.Settings;

// When updating class also update SiteSettingsDeploymentSource and SettingsStep.
public class SiteSettings : DocumentEntity, ISite
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

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
    public RouteValueDictionary HomeRoute { get; set; } = [];
    public bool AppendVersion { get; set; } = true;
    public CacheMode CacheMode { get; set; }

    public T As<T>() where T : new()
    {
        var name = typeof(T).Name;
        if (!IsReadOnly)
        {
            return this.As<T>(name);
        }

        if (_cache.TryGetValue(name, out var obj) && obj is T value)
        {
            return value;
        }

        var settings = this.As<T>(name);
        _cache[name] = settings;

        return settings;
    }

    internal void ClearCache()
    {
        _cache.Clear();
    }
}
