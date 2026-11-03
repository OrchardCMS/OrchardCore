using Microsoft.AspNetCore.Routing;
using OrchardCore.Entities;

namespace OrchardCore.Settings;

public interface ISite : IEntity
{
    string SiteName { get; set; }

    string PageTitleFormat { get; set; }

    string SiteSalt { get; set; }

    string SuperUser { get; set; }

    string Calendar { get; set; }

    string TimeZoneId { get; set; }

    ResourceDebugMode ResourceDebugMode { get; set; }

    bool UseCdn { get; set; }

    string CdnBaseUrl { get; set; }

    int PageSize { get; set; }

    int MaxPageSize { get; set; }

    int MaxPagedCount { get; set; }

    /// <summary>
    /// Gets or sets whether users can change the number of items displayed per page on listing pages.
    /// </summary>
    bool AllowPageSizeSelection { get; set; }

    /// <summary>
    /// Gets or sets the page size values a user can select from when <see cref="AllowPageSizeSelection"/> is enabled.
    /// </summary>
    int[] PageSizeOptions { get; set; }

    string BaseUrl { get; set; }

    RouteValueDictionary HomeRoute { get; set; }

    bool AppendVersion { get; set; }

    CacheMode CacheMode { get; set; }

    [Obsolete("Use TryGet<T> or GetOrCreate<T> instead.")]
    T As<T>() where T : new();

    T GetOrCreate<T>() where T : new();

    bool TryGet<T>(out T settings);
}
