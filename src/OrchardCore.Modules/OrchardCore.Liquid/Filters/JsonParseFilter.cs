using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Liquid.Filters
{
    public class JsonParseFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if(input.Type != FluidValues.String)
            {
                 throw new ArgumentException("The jsonparse filter accepts string as it's input");
            }

            return new ValueTask<FluidValue>(new ObjectValue(JObject.Parse(input.ToStringValue())));
        }
    }
}
