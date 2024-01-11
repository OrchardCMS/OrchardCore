using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Services;

namespace OrchardCore.Markdown.Filters
{
    public class Markdownify : ILiquidFilter
    {
        private readonly IMarkdownService _markdownService;

        public Markdownify(IMarkdownService markdownService)
        {
            _markdownService = markdownService;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            return new ValueTask<FluidValue>(new StringValue(_markdownService.ToHtml(input.ToStringValue())));
        }
    }
}
