using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.ShortCodes.Services;

public static class OrchardRazorHelperExtensions
{
    /// <summary>
    /// Applies short codes to html.
    /// </summary>
    /// <param name="html">The html to apply short codes.</param>
    public static async Task<IHtmlContent> HtmlToShortCodesAsync(this IOrchardHelper orchardHelper, string html)
    {
        var shortCodeService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShortCodeService>();

        html = await shortCodeService.ProcessAsync(html);

        return new HtmlString(html);
    }
}
