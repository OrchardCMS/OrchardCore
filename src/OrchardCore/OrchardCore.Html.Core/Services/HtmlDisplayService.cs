using Fluid.Values;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Services;
using Shortcodes;
using System.Text.Encodings.Web;

namespace OrchardCore.Html.Services;

public class HtmlDisplayService : IHtmlDisplayService
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IShortcodeService _shortcodeService;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public HtmlDisplayService(
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IShortcodeService shortcodeService,
        IHtmlSanitizerService htmlSanitizerService)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _htmlEncoder = htmlEncoder;
        _shortcodeService = shortcodeService;
        _htmlSanitizerService = htmlSanitizerService;
    }
    
    public async Task UpdateModelHtmlAsync<TModel>(
        TModel model,
        bool renderLiquid,
        Context shortcodeContext,
        bool sanitizeHtml)
        where TModel : HtmlViewModelBase
    {
        if (renderLiquid)
        {
            model.Html = await _liquidTemplateManager.RenderStringAsync(model.Html, _htmlEncoder, model,
                new Dictionary<string, FluidValue>
                {
                    [nameof(model.ContentItem)] = new ObjectValue(model.ContentItem),
                });
        }

        if (shortcodeContext != null)
        {
            shortcodeContext[nameof(model.ContentItem)] = model.ContentItem;
            model.Html = await _shortcodeService.ProcessAsync(model.Html, shortcodeContext);
        }
        
        if (sanitizeHtml)
        {
            model.Html = _htmlSanitizerService.Sanitize(model.Html);
        }
    }
}
