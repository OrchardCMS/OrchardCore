using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Markdown.Filters
{
    public class Markdownify : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            return Task.FromResult<FluidValue>(new StringValue(Markdig.Markdown.ToHtml(input.ToStringValue())));
        }
    }
}
