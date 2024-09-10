using System.Text.Json;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Liquid.Filters;

public static class JsonFilter
{
    public static ValueTask<FluidValue> Json(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        var formatting = JOptions.Default;
        if (arguments.At(0).ToBooleanValue())
        {
            formatting = JOptions.Indented;
        }

        switch (input.Type)
        {
            case FluidValues.Array:
                return ValueTask.FromResult<FluidValue>(new StringValue(JConvert.SerializeObject(input.Enumerate(context).Select(o => o.ToObjectValue()), formatting)));

            case FluidValues.Boolean:
                return ValueTask.FromResult<FluidValue>(new StringValue(JConvert.SerializeObject(input.ToBooleanValue(), formatting)));

            case FluidValues.Nil:
                return ValueTask.FromResult<FluidValue>(StringValue.Create("null"));

            case FluidValues.Number:
                return ValueTask.FromResult<FluidValue>(new StringValue(JConvert.SerializeObject(input.ToNumberValue(), formatting)));

            case FluidValues.DateTime:
            case FluidValues.Dictionary:
            case FluidValues.Object:
                return ValueTask.FromResult<FluidValue>(new StringValue(JConvert.SerializeObject(input.ToObjectValue(), formatting)));

            case FluidValues.String:
                var stringValue = input.ToStringValue();

                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return ValueTask.FromResult(input);
                }

                return ValueTask.FromResult<FluidValue>(new StringValue(JConvert.SerializeObject(stringValue, formatting)));
        }

        throw new NotSupportedException("Unrecognized FluidValue");
    }
}
