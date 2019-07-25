using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Liquid;

public static class LiquidRazorHelperExtensions
{
    /// <summary>
    /// Parses a liquid string to HTML.
    /// </summary>
    /// <param name="liquid">The liquid to parse.</param>
    /// <param name="model"></param>
    /// <summary>
    public static async Task<IHtmlContent> LiquidToHtml(this IOrchardHelper orchardHelper, string liquid, object model = null)
    {
        var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();

        var context = new TemplateContext();

        if (model != null)
        {
            context.MemberAccessStrategy.Register(model.GetType());
            context.LocalScope.SetValue("Model", model);
        }

        var content = await liquidTemplateManager.RenderAsync(liquid, context);
        return new HtmlString(content);
    }
}

