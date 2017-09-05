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

        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var text = input.ToStringValue();

            return Task.FromResult<FluidValue>(new StringValue(_slugService.Slugify(text)));
        }
    }
}
