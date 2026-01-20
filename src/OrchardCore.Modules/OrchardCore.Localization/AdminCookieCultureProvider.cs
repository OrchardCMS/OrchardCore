using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;
using OrchardCore.Routing;

namespace OrchardCore.Localization;

public class AdminCookieCultureProvider : CookieRequestCultureProvider
{
    public const string CookieNamePrefix = "admin_culture_";

    private readonly PathString _adminPath;

    public AdminCookieCultureProvider(ShellSettings shellSettings, AdminOptions adminOptions)
    {
        CookieName = MakeCookieName(shellSettings);
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

    public static string MakeCookieName(ShellSettings shellSettings) => CookieNamePrefix + shellSettings.VersionId;

    public static string MakeCookiePath(HttpContext httpContext) => httpContext.Request.PathBase.HasValue
        ? httpContext.Request.PathBase.ToString()
        : "/";
}
