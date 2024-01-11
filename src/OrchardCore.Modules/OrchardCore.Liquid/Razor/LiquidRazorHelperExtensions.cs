using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Liquid;

#pragma warning disable CA1050 // Declare types in namespaces
public static class LiquidRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Parses a liquid string to HTML.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="liquid"></param>
    public static Task<IHtmlContent> LiquidToHtmlAsync(this IOrchardHelper orchardHelper, string liquid)
    {
        return orchardHelper.LiquidToHtmlAsync(liquid, null);
    }

    /// <summary>
    /// Parses a liquid string to HTML.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="liquid">The liquid to parse.</param>
    /// <param name="model">A model to bind against.</param>
    public static async Task<IHtmlContent> LiquidToHtmlAsync(this IOrchardHelper orchardHelper, string liquid, object model)
    {
        var serviceProvider = orchardHelper.HttpContext.RequestServices;

        var liquidTemplateManager = serviceProvider.GetRequiredService<ILiquidTemplateManager>();
        var htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();

        var result = await liquidTemplateManager.RenderHtmlContentAsync(liquid, htmlEncoder, model);
        return result;
    }
}
