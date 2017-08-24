using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace Orchard.Liquid
{
    public interface ILiquidFilter
    {
        Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx);
    }
}
