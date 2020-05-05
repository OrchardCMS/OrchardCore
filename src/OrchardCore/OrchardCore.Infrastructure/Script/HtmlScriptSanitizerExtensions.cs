using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Infrastructure.Script;

public static class HtmlScriptSanitizerExtensions
{
    /// <summary>
    /// Santizies a string of html.
    /// </summary>
    /// <param name="html">The html to sanitize.</param>
    public static async Task<IHtmlContent> SanitizeHtmlAsync(this IOrchardHelper orchardHelper, string html)
    {
        var safeCodeFilterManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ISafeCodeFilterManager>();

        html = await safeCodeFilterManager.ProcessAsync(html);

        var sanitizer = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHtmlScriptSanitizer>();
        html = sanitizer.Sanitize(html);

        return new HtmlString(html);
    }
}
