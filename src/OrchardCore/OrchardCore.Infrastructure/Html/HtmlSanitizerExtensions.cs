using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Infrastructure.Html;

public static class HtmlSanitizerExtensions
{
    /// <summary>
    /// Sanitizes a string of html.
    /// </summary>
    /// <param name="html">The html to sanitize.</param>
    public static async Task<IHtmlContent> SanitizeHtmlAsync(this IOrchardHelper orchardHelper, string html)
    {
        var safeCodeFilterManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ISafeCodeFilterManager>();

        html = await safeCodeFilterManager.ProcessAsync(html);

        var sanitizer = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHtmlSanitizerService>();
        html = sanitizer.Sanitize(html);

        return new HtmlString(html);
    }
}
