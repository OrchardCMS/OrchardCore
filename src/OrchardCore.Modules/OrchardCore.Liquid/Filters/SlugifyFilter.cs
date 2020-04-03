using Fluid;
using Fluid.Values;
using System.Threading.Tasks;

namespace OrchardCore.Liquid.Filters
{
    public class SlugifyFilter : ILiquidFilter
    {
        private readonly ISlugService _slugService;

        public SlugifyFilter(ISlugService slugService)
        {
            _slugService = slugService;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var text = input.ToStringValue();

            return new ValueTask<FluidValue>(new StringValue(_slugService.Slugify(text)));
        }
    }
}
