using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Media.Filters
{
    public static class MediaFilters
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static ValueTask<FluidValue> ImgTag(FluidValue input, FilterArguments arguments, TemplateContext ctx)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var url = input.ToStringValue();

            var imgTag = $"<img src=\"{url}\"";

            foreach (var name in arguments.Names)
            {
                imgTag += $" {name.Replace('_', '-')}=\"{arguments[name].ToStringValue()}\"";
            }

            imgTag += " />";

            return new ValueTask<FluidValue>(new StringValue(imgTag) { Encode = false });
        }
    }
}
