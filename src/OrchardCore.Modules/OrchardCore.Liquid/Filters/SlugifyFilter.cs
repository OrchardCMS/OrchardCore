using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Modules.Services;

namespace OrchardCore.Liquid.Filters
{
    public class SlugifyFilter : ILiquidFilter
    {
        private readonly ISlugService _slugService;

        public SlugifyFilter(ISlugService slugService)
        {
            _slugService = slugService;
        }
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var text = input.ToStringValue();

            return new StringValue(_slugService.Slugify(text));
        }
    }
}
