using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.Themes.TheAdmin.Services;

/// <summary>
/// Serves the stylesheet of TheAdmin's schemes. It holds colors, fonts and scales only, and pages that need no sign in
/// (e.g. the log in page) use it too, so it is public. It is built once per change of the schemes and served from
/// memory. A request naming the current version (a short hash of the stylesheet) is cached by the browser for a year:
/// the layouts link to it with that version, which changes when a scheme is saved or the code that writes the
/// stylesheet changes, so a change is picked up on the next page.
/// </summary>
public static class AdminSchemeEndpoints
{
    /// <summary>
    /// The path of the stylesheet, relative to the tenant.
    /// </summary>
    public const string StylesheetPath = "TheAdmin/schemes.css";

    public static IEndpointRouteBuilder MapAdminSchemeStylesheet(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(StylesheetPath, async (HttpContext context) =>
        {
            var store = context.RequestServices.GetRequiredService<AdminSchemeStore>();
            var (css, version) = await store.GetStylesheetAsync();
            var requested = context.Request.Query["v"].ToString();

            context.Response.Headers[HeaderNames.CacheControl] = string.Equals(requested, version, StringComparison.Ordinal)
                ? "public, max-age=31536000, immutable"
                : "no-cache";

            return Results.Text(css, "text/css; charset=utf-8");
        });

        return routes;
    }
}
