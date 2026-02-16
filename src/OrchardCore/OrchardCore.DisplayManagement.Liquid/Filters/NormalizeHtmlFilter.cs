using Fluid;
using Fluid.Values;
using OrchardCore.DisplayManagement;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters;

public class NormalizeHtmlFilter : ILiquidFilter
{
    private readonly IHtmlNormalizer _htmlNormalizer;

    public NormalizeHtmlFilter(IHtmlNormalizer htmlNormalizer)
    {
        _htmlNormalizer = htmlNormalizer;
    }

    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        var html = input.ToStringValue();
        var sanitize = arguments.Count > 0 && arguments.At(0).ToBooleanValue();

        foreach (var name in arguments.Names)
        {
            if (string.Equals(name, "sanitize", StringComparison.OrdinalIgnoreCase))
            {
                sanitize = arguments[name].ToBooleanValue();
                break;
            }
        }

        html = _htmlNormalizer.Normalize(html, sanitize) ?? string.Empty;

        return ValueTask.FromResult<FluidValue>(new StringValue(html));
    }
}
