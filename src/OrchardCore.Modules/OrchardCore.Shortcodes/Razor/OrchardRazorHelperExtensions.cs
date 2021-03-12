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
    /// Applies shortcodes to html.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/></param>
    /// <param name="html">The html to apply shortcodes.</param>
    /// <param name="model">The ambient shape view model.</param>
    public static async Task<IHtmlContent> HtmlToShortcodesAsync(this IOrchardHelper orchardHelper, string html, object model = null)
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
}
