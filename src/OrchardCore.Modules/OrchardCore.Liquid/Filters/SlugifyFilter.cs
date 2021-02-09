using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Liquid.Filters
{
    public static class SlugifyFilter
    {
        public static ValueTask<FluidValue> Slugify(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var orchardContext = (LiquidTemplateContext)ctx;
            var slugService = orchardContext.Services.GetRequiredService<ISlugService>();
            var text = input.ToStringValue();

            return new StringValue(slugService.Slugify(text));
        }
    }
}
