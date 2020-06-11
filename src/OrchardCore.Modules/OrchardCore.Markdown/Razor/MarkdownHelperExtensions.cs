using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.ShortCodes.Services;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Services;

public static class ContentRazorHelperExtensions
{
    /// <summary>
    /// Converts Markdown string to HTML.
    /// </summary>
    /// <param name="markdown">The markdown to convert.</param>
    public static async Task<IHtmlContent> MarkdownToHtmlAsync(this IOrchardHelper orchardHelper, string markdown, bool sanitize = true)
    {
        var shortCodeService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShortCodeService>();
        var markdownService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IMarkdownService>();

        // The default Markdown option is to entity escape html
        // so filters must be run after the markdown has been processed.
        markdown = markdownService.ToHtml(markdown ?? "");

        // The liquid rendering is for backwards compatability and can be removed in a future version.
        if (!sanitize)
        {
            var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();
            var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();

            markdown = await liquidTemplateManager.RenderAsync(markdown, htmlEncoder);
        }

        markdown = await shortCodeService.ProcessAsync(markdown);

        if (sanitize)
        {
            var sanitizer = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHtmlSanitizerService>();
            markdown = sanitizer.Sanitize(markdown);
        }

        return new HtmlString(markdown);
    }
}
