using Ganss.Xss;

namespace OrchardCore.Infrastructure.Html;

public class HtmlSanitizerOptions
{
    public List<Action<HtmlSanitizer>> Configure { get; } = [];
}
