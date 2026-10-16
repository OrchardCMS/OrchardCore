using System.Collections.Concurrent;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Documents;
using OrchardCore.Entities;

namespace OrchardCore.Settings;

// When updating class also update SiteSettingsDeploymentSource and SettingsStep.
public class SiteSettings : DocumentEntity, ISite
{
    private static readonly object s_missing = new();
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public string Calendar { get; set; }
    public int MaxPagedCount { get; set; }
    public int MaxPageSize { get; set; }
    public int PageSize { get; set; }
    public bool AllowPageSizeSelection { get; set; }
    public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string TimeZoneId { get; set; }
    public ResourceDebugMode ResourceDebugMode { get; set; }
    public string SiteName { get; set; }
    public string SiteSalt { get; set; }
    public string PageTitleFormat { get; set; }
    public string SuperUser { get; set; }
    public bool UseCdn { get; set; }
    public RouteValueDictionary HomeRoute { get; set; } = [];
    public bool AppendVersion { get; set; } = true;
    public CacheMode CacheMode { get; set; }

    public string BaseUrl
    {
        get => BaseUri?.AbsoluteUri;
        set => BaseUri = string.IsNullOrWhiteSpace(value) ? null : new Uri(value, UriKind.Absolute);
    }

    public string CdnBaseUrl
    {
        get => CdnBaseUri?.AbsoluteUri;
        set => CdnBaseUri = string.IsNullOrWhiteSpace(value) ? null : new Uri(value, UriKind.Absolute);
    }
    
    public Uri BaseUri { get; set; }
    public Uri CdnBaseUri { get; set; }

    public T As<T>() where T : new()
        => GetOrCreate<T>();

    public T GetOrCreate<T>() where T : new()
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
            if (ReferenceEquals(obj, s_missing))
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

        _cache[name] = s_missing;
        settings = default;
        return false;
    }

    internal void ClearCache()
    {
        _cache.Clear();
    }
}
