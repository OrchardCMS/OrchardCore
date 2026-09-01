using Shortcodes;

namespace OrchardCore.Markdown.Services;

/// <summary>
/// Converts authored Markdown into final rendered HTML.
/// </summary>
public interface IMarkdownDisplayService
{
    /// <summary>
    /// Converts Markdown to HTML, processes shortcodes, and optionally sanitizes the result.
    /// </summary>
    /// <param name="markdown">The authored Markdown source.</param>
    /// <param name="shortcodeContext">The context used to process shortcodes.</param>
    /// <param name="sanitizeHtml">Whether to sanitize the final rendered HTML.</param>
    /// <returns>The final rendered HTML.</returns>
    Task<string> ToHtmlAsync(
        string markdown,
        Context shortcodeContext = null,
        bool sanitizeHtml = true);
}
