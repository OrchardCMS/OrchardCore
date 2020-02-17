using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Newtonsoft.Json;

namespace OrchardCore.Liquid.Filters
{
    public class JsonFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            switch (input.Type)
            {
                case FluidValues.Array:
                    return new ValueTask<FluidValue>(new StringValue(JsonConvert.SerializeObject(input.Enumerate().Select(o => o.ToObjectValue()))));

                case FluidValues.Boolean:
                    return new ValueTask<FluidValue>(new StringValue(JsonConvert.SerializeObject(input.ToBooleanValue())));

                case FluidValues.Nil:
                    return new ValueTask<FluidValue>(StringValue.Create("null"));

                case FluidValues.Number:
                    return new ValueTask<FluidValue>(new StringValue(JsonConvert.SerializeObject(input.ToNumberValue())));

                case FluidValues.DateTime:
                case FluidValues.Dictionary:
                case FluidValues.Object:
                    return new ValueTask<FluidValue>(new StringValue(JsonConvert.SerializeObject(input.ToObjectValue())));

                case FluidValues.String:
                    var stringValue = input.ToStringValue();

                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        return new ValueTask<FluidValue>(input);
                    }

                    return new ValueTask<FluidValue>(new StringValue(JsonConvert.SerializeObject(stringValue)));
            }

            throw new NotSupportedException("Unrecognized FluidValue");
        }
    }
}
