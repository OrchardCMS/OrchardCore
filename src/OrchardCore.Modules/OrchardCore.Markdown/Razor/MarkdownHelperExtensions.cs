using Microsoft.AspNetCore.Html;
using OrchardCore;

public static class ContentRazorHelperExtensions
{
    /// <summary>
    /// Converts Markdown string to HTML.
    /// </summary>
    /// <param name="markdown">The markdown to convert.</param>
    public static IHtmlContent MarkdownToHtml(this IOrchardHelper orchardHelper, string markdown)
    {
        return new HtmlString(Markdig.Markdown.ToHtml(markdown));
    }
}

