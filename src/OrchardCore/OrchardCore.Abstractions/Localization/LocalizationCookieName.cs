using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Localization;

/// <summary>
/// Contains a localization cookie names.
/// </summary>
public class LocalizationCookieName
{
    /// <summary>
    /// Gets the cookie name that used for the admin site.
    /// </summary>
    public static readonly string AdminSite = ".OrchardCore.AdminSiteCulture";

    /// <summary>
    /// Gets the cookie name that used for the site.
    /// </summary>
    public static readonly string Site = CookieRequestCultureProvider.DefaultCookieName;
}
