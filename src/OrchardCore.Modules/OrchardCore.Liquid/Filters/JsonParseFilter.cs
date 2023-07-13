using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Liquid.Filters
{
    public static class JsonParseFilter
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static ValueTask<FluidValue> JsonParse(FluidValue input, FilterArguments arguments, TemplateContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var parsedValue = JToken.Parse(input.ToStringValue());
            if (parsedValue.Type == JTokenType.Array)
            {
                return new ValueTask<FluidValue>(FluidValue.Create(parsedValue, context.Options));
            }
            return new ValueTask<FluidValue>(new ObjectValue(parsedValue));
        }
    }
}
