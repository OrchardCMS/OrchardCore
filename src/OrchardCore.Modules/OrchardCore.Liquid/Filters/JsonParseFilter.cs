using System.Text.Json;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Liquid.Filters;

public static class JsonParseFilter
{
    public static ValueTask<FluidValue> JsonParse(FluidValue input, FilterArguments _, TemplateContext context)
    {
        var parsedValue = JNode.Parse(input.ToStringValue());
        if (parsedValue.GetValueKind() == JsonValueKind.Array)
        {
            return ValueTask.FromResult(FluidValue.Create(parsedValue, context.Options));
        }

        return ValueTask.FromResult<FluidValue>(new ObjectValue(parsedValue));
    }
}
