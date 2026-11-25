using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;
using OrchardCore.Routing;

namespace OrchardCore.Localization;

public class AdminCookieCultureProvider : CookieRequestCultureProvider
{
    [Obsolete("Use the configured 'CookieNameOptions.AdminCulture' name instead. This constant will be removed in a future version.")]
    public const string CookieNamePrefix = CookieNames.AdminCulturePrefix;

    private readonly PathString _adminPath;

    [Obsolete("Use the constructor accepting the 'CookieNameOptions' instead, which uses the configured cookie name.")]
    public AdminCookieCultureProvider(ShellSettings shellSettings, AdminOptions adminOptions)
    {
        CookieName = CookieNames.AdminCulturePrefix + shellSettings.VersionId;
        _adminPath = new PathString("/" + adminOptions.AdminUrlPrefix.Trim('/'));
    }

    public AdminCookieCultureProvider(CookieNameOptions cookieNameOptions, AdminOptions adminOptions)
    {
        CookieName = cookieNameOptions.AdminCulture;
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

    [Obsolete("Use the configured 'CookieNameOptions.AdminCulture' name instead. This method will be removed in a future version.")]
    public static string MakeCookieName(ShellSettings shellSettings) => CookieNames.AdminCulturePrefix + shellSettings.VersionId;

    public static string MakeCookiePath(HttpContext httpContext) => httpContext.Request.PathBase.HasValue
        ? httpContext.Request.PathBase.ToString()
        : "/";
}
