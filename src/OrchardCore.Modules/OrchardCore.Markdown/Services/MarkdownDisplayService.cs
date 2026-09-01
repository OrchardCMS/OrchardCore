using OrchardCore.Infrastructure.Html;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Markdown.Services;

public sealed class MarkdownDisplayService : IMarkdownDisplayService
{
    private readonly IMarkdownService _markdownService;
    private readonly IShortcodeService _shortcodeService;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public MarkdownDisplayService(
        IMarkdownService markdownService,
        IShortcodeService shortcodeService,
        IHtmlSanitizerService htmlSanitizerService)
    {
        _markdownService = markdownService;
        _shortcodeService = shortcodeService;
        _htmlSanitizerService = htmlSanitizerService;
    }

    public async Task<string> ToHtmlAsync(
        string markdown,
        Context shortcodeContext = null,
        bool sanitizeHtml = true)
    {
        var html = _markdownService.ToHtml(markdown ?? string.Empty);

        html = await _shortcodeService.ProcessAsync(html, shortcodeContext);

        if (sanitizeHtml)
        {
            html = _htmlSanitizerService.Sanitize(html);
        }

        return html;
    }
}
