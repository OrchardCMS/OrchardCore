using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Infrastructure.Html;

public static class HtmlSanitizerRazorExtensions
{
    /// <summary>
    /// Sanitizes a string of html.
    /// </summary>
    /// <param name="html">The html to sanitize.</param>
    public static IHtmlContent SanitizeHtml(this IOrchardHelper orchardHelper, string html)
    {
        var sanitizer = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHtmlSanitizerService>();
        html = sanitizer.Sanitize(html);

        return new HtmlString(html);
    }
}
