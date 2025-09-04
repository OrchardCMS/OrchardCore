using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Services;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore;

public static class ContentRazorHelperExtensions
{
    /// <summary>
    /// Converts Markdown string to HTML.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="markdown">The markdown to convert.</param>
    /// <param name="sanitize">Whether to sanitize the markdown. Defaults to <see langword="true"/>.</param>
    public static async Task<IHtmlContent> MarkdownToHtmlAsync(this IOrchardHelper orchardHelper, string markdown, bool sanitize = true)
    {
        var shortcodeService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShortcodeService>();
        var markdownService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IMarkdownService>();

        // The default Markdown option is to entity escape html
        // so filters must be run after the markdown has been processed.
        markdown = markdownService.ToHtml(markdown ?? string.Empty);

        // The liquid rendering is for backwards compatibility and can be removed in a future version.
        if (!sanitize)
        {
            var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();
            var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();

            markdown = await liquidTemplateManager.RenderStringAsync(markdown, htmlEncoder);
        }

        // TODO: provide context argument (optional on this helper as with the liquid helper?).

        markdown = await shortcodeService.ProcessAsync(markdown);

        if (sanitize)
        {
            var sanitizer = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHtmlSanitizerService>();
            markdown = sanitizer.Sanitize(markdown);
        }

        return new HtmlString(markdown);
    }
}
