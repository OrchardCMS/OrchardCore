using System.Collections.Concurrent;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Documents;
using OrchardCore.Entities;

namespace OrchardCore.Settings;

// When updating class also update SiteSettingsDeploymentSource and SettingsStep.
public class SiteSettings : DocumentEntity, ISite
{
    private readonly object _missing = new();
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
        => TryGet<T>(out var settings) ? settings : new T();

    public bool TryGet<T>(out T settings)
    {
        var name = typeof(T).Name;

        if (!IsReadOnly)
        {
            return this.TryGet(name, out settings);
        }

        if (_cache.TryGetValue(name, out var obj))
        {
            if (ReferenceEquals(obj, _missing))
            {
                settings = default;
                return false;
            }

            settings = (T)obj;
            return true;
        }

        if (this.TryGet(name, out settings))
        {
            _cache[name] = settings;
            return true;
        }

        _cache[name] = _missing;
        settings = default;
        return false;
    }

    internal void ClearCache()
    {
        _cache.Clear();
    }
}
