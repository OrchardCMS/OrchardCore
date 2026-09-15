using OrchardCore.Html.ViewModels;
using Shortcodes;

namespace OrchardCore.Html.Services;

/// <summary>
/// Processes authored HTML into final rendered HTML.
/// </summary>
public interface IHtmlDisplayService
{
    /// <summary>
    /// Processes shortcodes in a model's HTML and optionally sanitizes the result.
    /// </summary>
    /// <typeparam name="TModel">The HTML view model type.</typeparam>
    /// <param name="model">The model containing authored HTML.</param>
    /// <param name="shortcodeContext">The context used to process shortcodes.</param>
    /// <param name="sanitizeHtml">Whether to sanitize the final rendered HTML.</param>
    /// <returns>A task representing the operation.</returns>
    Task UpdateModelHtmlAsync<TModel>(
        TModel model,
        Context shortcodeContext,
        bool sanitizeHtml)
        where TModel : HtmlViewModelBase;
}
