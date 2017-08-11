using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace Orchard.Liquid.Filters
{
    public class SetPropertyFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var obj = input.ToObjectValue() as dynamic;

            if (obj != null)
            {
                obj[arguments.At(0).ToStringValue()] = arguments.At(1).ToStringValue();
            }

            return Task.FromResult(input);
        }
    }
}
