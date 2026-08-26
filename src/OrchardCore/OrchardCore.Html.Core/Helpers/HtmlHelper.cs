using Fluid.Values;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Services;
using Shortcodes;
using System.Text.Encodings.Web;

namespace OrchardCore.Html.Core.Helpers;

public static class HtmlHelper
{
    public static async Task UpdateModelHtmlAsync<TModel>(
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IShortcodeService shortcodeService,
        IHtmlSanitizerService htmlSanitizerService,
        TModel model,
        bool renderLiquid,
        Context shortcodeContext,
        bool sanitizeHtml)
        where TModel : HtmlViewModelBase
    {
        if (renderLiquid)
        {
            model.Html = await liquidTemplateManager.RenderStringAsync(model.Html, htmlEncoder, model,
                new Dictionary<string, FluidValue>
                {
                    [nameof(model.ContentItem)] = new ObjectValue(model.ContentItem),
                });
        }

        if (shortcodeContext != null)
        {
            shortcodeContext[nameof(model.ContentItem)] = model.ContentItem;
            model.Html = await shortcodeService.ProcessAsync(model.Html, shortcodeContext);
        }
        
        if (sanitizeHtml)
        {
            model.Html = htmlSanitizerService.Sanitize(model.Html);
        }
    }
}
