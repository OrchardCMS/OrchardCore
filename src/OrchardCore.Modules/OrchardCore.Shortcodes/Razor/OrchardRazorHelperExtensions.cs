using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Shortcodes.Services;

public static class OrchardRazorHelperExtensions
{
    /// <summary>
    /// Applies shortcodes to html.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/></param>
    /// <param name="html">The html to apply shortcodes.</param>
    public static async Task<IHtmlContent> HtmlToShortcodesAsync(this IOrchardHelper orchardHelper, string html)
    {
        var shortcodeService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShortcodeService>();

        // TODO provide optional context argument.

        html = await shortcodeService.ProcessAsync(html);

        return new HtmlString(html);
    }
}
