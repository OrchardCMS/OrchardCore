using OrchardCore.Html.ViewModels;
using Shortcodes;

namespace OrchardCore.Html.Services;

public interface IHtmlDisplayService
{
    Task UpdateModelHtmlAsync<TModel>(
        TModel model,
        bool renderLiquid,
        Context shortcodeContext,
        bool sanitizeHtml)
        where TModel : HtmlViewModelBase;
}
