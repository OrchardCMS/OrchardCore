using System.IO;
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
    /// <param name="liquid"></param>
    /// <returns></returns>
    public static async Task<IHtmlContent> LiquidToHtml(this IOrchardHelper orchardHelper, string liquid)
    {
        return await orchardHelper.LiquidToHtml(liquid, null);
    }

    /// <summary>
    /// Parses a liquid string to HTML.
    /// </summary>
    /// <param name="liquid">The liquid to parse.</param>
    /// <param name="model">A model to bind against.</param>
    /// <summary>
    public static async Task<IHtmlContent> LiquidToHtml(this IOrchardHelper orchardHelper, string liquid, object model)
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

    /// <summary>
    /// Parses a liquid string.
    /// </summary>
    /// <param name="liquid">The liquid to parse.</param>
    /// <summary>
    public static async Task<string> LiquidToRaw(this IOrchardHelper orchardHelper, string liquid)
    {
        return await orchardHelper.LiquidToRaw(liquid, null);
    }

    /// <summary>
    /// Parses a liquid string.
    /// </summary>
    /// <param name="liquid">The liquid to parse.</param>
    /// <param name="model">A model to bind against.</param>
    /// <summary>
    public static async Task<string> LiquidToRaw(this IOrchardHelper orchardHelper, string liquid, object model)
    {
        var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();

        var context = new TemplateContext();

        if (model != null)
        {
            context.MemberAccessStrategy.Register(model.GetType());
            context.LocalScope.SetValue("Model", model);
        }

        return await liquidTemplateManager.RenderAsync(liquid, context);
    }
}

