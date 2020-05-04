using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Infrastructure.Script;
using OrchardCore.Liquid;

public static class ContentRazorHelperExtensions
{
    /// <summary>
    /// Converts Markdown string to HTML.
    /// </summary>
    /// <param name="markdown">The markdown to convert.</param>
    public static async Task<IHtmlContent> MarkdownToHtmlAsync(this IOrchardHelper orchardHelper, string markdown, bool sanitize = true)
    {
        var safeCodeFilterManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ISafeCodeFilterManager>();

        if (!sanitize)
        {
            var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();
            var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();

            markdown = await liquidTemplateManager.RenderAsync(markdown, htmlEncoder);
        }

        markdown = await safeCodeFilterManager.ProcessAsync(markdown);
        markdown = Markdig.Markdown.ToHtml(markdown);

        if (sanitize)
        {
            var sanitizer = orchardHelper.HttpContext.RequestServices.GetRequiredService<IHtmlScriptSanitizer>();
            markdown = sanitizer.Sanitize(markdown);
        }

        return new HtmlString(markdown);
    }
}
