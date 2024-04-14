using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Media.Filters
{
    public static class MediaFilters
    {
        public static ValueTask<FluidValue> ImgTag(FluidValue input, FilterArguments arguments, TemplateContext _)
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
