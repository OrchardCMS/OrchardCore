using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Infrastructure.Html;

#pragma warning disable CA1050 // Declare types in namespaces
public static class HtmlSanitizerRazorExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Sanitizes a string of html.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="html">The html to sanitize.</param>
    public static IHtmlContent SanitizeHtml(this IOrchardHelper orchardHelper, string html)
    {
        var sanitizer = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHtmlSanitizerService>();
        html = sanitizer.Sanitize(html);

        return new HtmlString(html);
    }
}
