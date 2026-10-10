using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Html.Services;

public class HtmlDisplayService : IHtmlDisplayService
{
    private readonly IShortcodeService _shortcodeService;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public HtmlDisplayService(
        IShortcodeService shortcodeService,
        IHtmlSanitizerService htmlSanitizerService)
    {
        _shortcodeService = shortcodeService;
        _htmlSanitizerService = htmlSanitizerService;
    }

    public async Task UpdateModelHtmlAsync<TModel>(
        TModel model,
        Context shortcodeContext,
        bool sanitizeHtml)
        where TModel : HtmlViewModelBase
    {
        model.Html ??= string.Empty;

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
