using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Markdown.Services;

namespace OrchardCore;

public static class ContentRazorHelperExtensions
{
    /// <summary>
    /// Converts Markdown string to HTML.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="markdown">The markdown to convert.</param>
    /// <param name="sanitize">Whether to sanitize the rendered HTML. Defaults to <see langword="true"/>.</param>
    public static async Task<IHtmlContent> MarkdownToHtmlAsync(
        this IOrchardHelper orchardHelper,
        string markdown,
        bool sanitize = true)
    {
        var markdownDisplayService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IMarkdownDisplayService>();
        var html = await markdownDisplayService.ToHtmlAsync(markdown, sanitizeHtml: sanitize);

        return new HtmlString(html);
    }
}
