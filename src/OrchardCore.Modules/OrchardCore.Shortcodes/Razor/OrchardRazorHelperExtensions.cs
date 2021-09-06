using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

public static class OrchardRazorHelperExtensions
{
    /// <summary>
    /// Processes shortcodes contained inside html.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/></param>
    /// <param name="html">The html string contained shortcodes.</param>
    /// <param name="model">The ambient shape view model.</param>
    public static async Task<IHtmlContent> ShortcodesToHtmlAsync(this IOrchardHelper orchardHelper, string html, object model = null)
    {
        var shortcodeService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShortcodeService>();

        var context = new Context();

        // Retrieve the 'ContentItem' from the ambient shape view model.
        if (model is Shape shape && shape.TryGetProperty("ContentItem", out object contentItem))
        {
            context["ContentItem"] = contentItem;
        }
        else
        {
            context["ContentItem"] = null;
        }

        html = await shortcodeService.ProcessAsync(html, context);

        return new HtmlString(html);
    }

    [Obsolete("Replaced by ShortcodesToHtmlAsync")]
    public static Task<IHtmlContent> HtmlToShortcodesAsync(this IOrchardHelper orchardHelper, string html, object model = null)
        => orchardHelper.ShortcodesToHtmlAsync(html, model);
}
