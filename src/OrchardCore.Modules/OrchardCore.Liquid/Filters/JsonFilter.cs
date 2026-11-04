using System.Text.Json;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Liquid.Filters;

public static class JsonFilter
{
    public static async ValueTask<FluidValue> Json(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        var formatting = JOptions.Default;
        if (arguments.At(0).ToBooleanValue(context))
        {
            formatting = JOptions.Indented;
        }

        switch (input.Type)
        {
            case FluidValues.Array:
                return StringValue.Create(JConvert.SerializeObject(await input.EnumerateAsync(context).Select(o => o.ToObjectValue(context)).ToArrayAsync(), formatting));

            case FluidValues.Boolean:
                return StringValue.Create(JConvert.SerializeObject(input.ToBooleanValue(context), formatting));

            case FluidValues.Nil:
                return StringValue.Create("null");

            case FluidValues.Number:
                return StringValue.Create(JConvert.SerializeObject(input.ToNumberValue(context), formatting));

            case FluidValues.DateTime:
            case FluidValues.Dictionary:
            case FluidValues.Object:
                return StringValue.Create(JConvert.SerializeObject(input.ToObjectValue(context), formatting));

            case FluidValues.String:
                var stringValue = input.ToStringValue(context);

                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return input;
                }

                return StringValue.Create(JConvert.SerializeObject(stringValue, formatting));
        }

        throw new NotSupportedException("Unrecognized FluidValue");
    }
}
