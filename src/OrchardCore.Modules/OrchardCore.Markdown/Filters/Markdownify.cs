using Fluid;
using Fluid.Values;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Services;

namespace OrchardCore.Markdown.Filters;

public class Markdownify : ILiquidFilter
{
    private readonly IMarkdownService _markdownService;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public Markdownify(
        IMarkdownService markdownService,
        IHtmlSanitizerService htmlSanitizerService)
    {
        _markdownService = markdownService;
        _htmlSanitizerService = htmlSanitizerService;
    }

    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        var html = _markdownService.ToHtml(input.ToStringValue());
        html = _htmlSanitizerService.Sanitize(html);

        return ValueTask.FromResult<FluidValue>(new StringValue(html));
    }
}
