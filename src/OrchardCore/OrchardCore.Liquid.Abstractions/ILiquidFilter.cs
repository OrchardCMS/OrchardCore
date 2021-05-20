using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Liquid
{
    public interface ILiquidFilter
    {
        ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context);
    }
}
