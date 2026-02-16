using HtmlAgilityPack;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.DisplayManagement.Html;

internal sealed class DefaultHtmlNormalizer : IHtmlNormalizer
{
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public DefaultHtmlNormalizer(IHtmlSanitizerService htmlSanitizerService)
    {
        _htmlSanitizerService = htmlSanitizerService;
    }

    public string Normalize(string html, bool sanitize = false)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var doc = new HtmlDocument
        {
            // Fix nested tags.
            OptionFixNestedTags = true,

            // Auto-close tags at the end.
            OptionAutoCloseOnEnd = true,

            // Enable syntax checking.
            OptionCheckSyntax = true,

            // Optional: keep it HTML style.
            OptionOutputAsXml = false,
        };

        doc.LoadHtml(html);

        // This will automatically fix some of the malformed structure.
        var normalizedHtml = doc.DocumentNode.OuterHtml;

        if (sanitize)
        {
            return _htmlSanitizerService.Sanitize(normalizedHtml);
        }

        return normalizedHtml;
    }
}
