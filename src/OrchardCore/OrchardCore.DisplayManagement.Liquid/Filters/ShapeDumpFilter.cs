using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public class ShapeDumpFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var obj = input.ToObjectValue();
            // Will throw a InvalidCastException if object is not a shape.
            var iShape = (IShape)obj;

            return new ValueTask<FluidValue>(new StringValue(iShape.ShapeDump().ToString()));
        }
    }
}
