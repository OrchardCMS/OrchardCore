using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.Themes.Schemes.Services;

/// <summary>
/// Serves the stylesheet of the schemes of each theme that supports them. It holds colors, fonts and scales only, and
/// pages that need no sign in (e.g. the log in page of the admin) use it too, so it is public. A request naming the
/// current version is cached by the browser for a year: the layouts link to it with the version of the document,
/// which changes on every save, so a change is picked up on the next page.
/// </summary>
public static class ThemeSchemeEndpoints
{
    /// <summary>
    /// The route of the stylesheets, relative to the tenant.
    /// </summary>
    public const string StylesheetRoute = "Themes/schemes/{themeId}.css";

    /// <summary>
    /// The path of the stylesheet of a theme, relative to the tenant.
    /// </summary>
    public static string GetStylesheetPath(string themeId)
        => $"Themes/schemes/{Uri.EscapeDataString(themeId)}.css";

    public static IEndpointRouteBuilder MapThemeSchemeStylesheets(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(StylesheetRoute, async (string themeId, HttpContext context) =>
        {
            var store = context.RequestServices.GetRequiredService<ThemeSchemeStore>();

            if (!store.IsRegistered(themeId))
            {
                return Results.NotFound();
            }

            var (css, version) = await store.GetStylesheetAsync(themeId);
            var requested = context.Request.Query["v"].ToString();

            context.Response.Headers[HeaderNames.CacheControl] = string.Equals(requested, version, StringComparison.Ordinal)
                ? "public, max-age=31536000, immutable"
                : "no-cache";

            return Results.Text(css, "text/css; charset=utf-8");
        });

        return routes;
    }
}
