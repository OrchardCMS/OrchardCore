using Fluid;
using Fluid.Values;

namespace OrchardCore.Media.Filters;

public static class MediaFilters
{
    public static ValueTask<FluidValue> ImgTag(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        var url = input.ToStringValue(context);

        var imgTag = $"<img src=\"{url}\"";

        foreach (var name in arguments.Names)
        {
            imgTag += $" {name.Replace('_', '-')}=\"{arguments[name].ToStringValue(context)}\"";
        }

        imgTag += " />";

        return ValueTask.FromResult<FluidValue>(StringValue.Create(imgTag, encode: false));
    }
}
