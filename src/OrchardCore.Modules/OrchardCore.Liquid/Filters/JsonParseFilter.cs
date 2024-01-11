using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Liquid.Filters
{
    public static class JsonParseFilter
    {
        public static ValueTask<FluidValue> JsonParse(FluidValue input, FilterArguments _, TemplateContext context)
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
