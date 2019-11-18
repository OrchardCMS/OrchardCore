using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Liquid;

public static class ContentRazorHelperExtensions
{
    /// <summary>
    /// Converts Markdown string to HTML.
    /// </summary>
    /// <param name="markdown">The markdown to convert.</param>
    public static async Task<IHtmlContent> MarkdownToHtmlAsync(this IOrchardHelper orchardHelper, string markdown)
    {
        var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();
        var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetService<HtmlEncoder>();

        var context = new TemplateContext();

        markdown = await liquidTemplateManager.RenderAsync(markdown, NullHtmlEncoder.Default, context);
        return new HtmlString(htmlEncoder.Encode(Markdig.Markdown.ToHtml(markdown)));
    }
}
