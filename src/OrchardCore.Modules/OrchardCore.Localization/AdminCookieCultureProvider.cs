using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;
using OrchardCore.Routing;

namespace OrchardCore.Localization;

public class AdminCookieCultureProvider : CookieRequestCultureProvider
{
    [Obsolete("Use the configured 'LocalizationCookieOptions.AdminCulture' name instead. This constant will be removed in a future version.")]
    public const string CookieNamePrefix = LocalizationCookieOptions.DefaultAdminCulturePrefix;

    private readonly PathString _adminPath;

    [Obsolete("Use the constructor accepting the 'LocalizationCookieOptions' instead, which uses the configured cookie name.")]
    public AdminCookieCultureProvider(ShellSettings shellSettings, AdminOptions adminOptions)
    {
        CookieName = LocalizationCookieOptions.DefaultAdminCulturePrefix + shellSettings.VersionId;
        _adminPath = new PathString("/" + adminOptions.AdminUrlPrefix.Trim('/'));
    }

    public AdminCookieCultureProvider(LocalizationCookieOptions cookieOptions, AdminOptions adminOptions)
    {
        CookieName = cookieOptions.AdminCulture;
        _adminPath = new PathString("/" + adminOptions.AdminUrlPrefix.Trim('/'));
    }

    public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithNormalizedSegments(_adminPath))
        {
            return base.DetermineProviderCultureResult(httpContext);
        }

        return NullProviderCultureResult;
    }

    [Obsolete("Use the configured 'LocalizationCookieOptions.AdminCulture' name instead. This method will be removed in a future version.")]
    public static string MakeCookieName(ShellSettings shellSettings) => LocalizationCookieOptions.DefaultAdminCulturePrefix + shellSettings.VersionId;

    public static string MakeCookiePath(HttpContext httpContext) => httpContext.Request.PathBase.HasValue
        ? httpContext.Request.PathBase.ToString()
        : "/";
}
