using Microsoft.Extensions.Localization;

namespace OrchardCore.Settings.Services;

internal static class SiteSettingsValidator
{
    public static string ValidatePageSize(IStringLocalizer localizer, int pageSize, int maxPageSize)
    {
        if (pageSize < 1)
        {
            return localizer["The page size must be greater than zero."];
        }

        return maxPageSize > 0 && pageSize > maxPageSize
            ? localizer["The page size must be less than or equal to {0}.", maxPageSize]
            : null;
    }

    public static string ValidateMaxPageSize(IStringLocalizer localizer, int maxPageSize)
        => maxPageSize < 0 ? localizer["The maximum page size must be zero or greater."] : null;

    public static string ValidateMaxPagedCount(IStringLocalizer localizer, int maxPagedCount)
        => maxPagedCount < 0 ? localizer["The maximum paged count must be zero or greater."] : null;

    public static string ValidateBaseUrl(IStringLocalizer localizer, string baseUrl)
        => !string.IsNullOrEmpty(baseUrl) && !IsValidUrl(baseUrl, restrictScheme: true)
            ? localizer["The Base url must be a fully qualified URL."]
            : null;

    public static string ValidateCdnBaseUrl(IStringLocalizer localizer, string cdnBaseUrl)
        => !string.IsNullOrEmpty(cdnBaseUrl) && !IsValidUrl(cdnBaseUrl, restrictScheme: false)
            ? localizer["The CDN base url must be a fully qualified URL."]
            : null;

    // The site base URL is always HTTP or HTTPS, while the CDN base URL could be something else (like FTP) as well.
    public static bool IsValidUrl(string url, bool restrictScheme)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (!restrictScheme || uri.Scheme is "http" or "https") &&
            string.IsNullOrEmpty(uri.Query) &&
            string.IsNullOrEmpty(uri.Fragment);
}
