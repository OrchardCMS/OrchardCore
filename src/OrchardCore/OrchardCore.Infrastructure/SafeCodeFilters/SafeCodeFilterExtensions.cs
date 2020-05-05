using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Infrastructure.SafeCodeFilters;

public static class SafeCodeFilterExtensions
{
    /// <summary>
    /// Applies safe code filters to html.
    /// </summary>
    /// <param name="html">The html to apply filters.</param>
    public static async Task<IHtmlContent> SafeCodeFilterAsync(this IOrchardHelper orchardHelper, string html)
    {
        var safeCodeFilterManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ISafeCodeFilterManager>();

        html = await safeCodeFilterManager.ProcessAsync(html);

        return new HtmlString(html);
    }
}
