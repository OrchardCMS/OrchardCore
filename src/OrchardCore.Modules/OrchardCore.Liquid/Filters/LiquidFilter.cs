using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Liquid.Filters
{
    public class LiquidFilter : ILiquidFilter
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;

        public LiquidFilter(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var content = await _liquidTemplateManager.RenderStringAsync(input.ToStringValue(), _htmlEncoder, arguments.At(0));

            return new StringValue(content, false);
        }
    }
}
