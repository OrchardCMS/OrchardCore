using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Orchard.Liquid;

namespace Orchard.Media.Filters
{
    public class MediaUrlFilter : ILiquidFilter
    {
        private readonly IMediaFileStore _mediaFileStore;

        public MediaUrlFilter(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();
            var imageUrl = _mediaFileStore.GetPublicUrl(url);

            return Task.FromResult<FluidValue>(new StringValue(imageUrl ?? url));
        }
    }

    public class ImageTagFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();
            var alt = arguments.At(0).Or(arguments["tag"]);
            var css = arguments.At(1).Or(arguments["class"]);

            var imgTag = $"<img src=\"{url}\"";

            if (!alt.IsNil())
            {
                imgTag += $" alt=\"{alt.ToStringValue()}\"";
            }

            if (!css.IsNil())
            {
                imgTag += $" class=\"{css.ToStringValue()}\"";
            }

            imgTag += " />";

            return Task.FromResult<FluidValue>(new StringValue(imgTag) { Encode = false });
        }
    }

    public class ResizeUrlFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();

            if (!url.Contains("?"))
            {
                url += "?";
            }

            var width = arguments.At(0).Or(arguments["width"]);
            var height = arguments.At(1).Or(arguments["height"]);
            var mode = arguments.At(2).Or(arguments["mode"]);

            if (!width.IsNil())
            {
                url += "&width=" + width.ToStringValue();
            }

            if (!height.IsNil())
            {
                url += "&height=" + height.ToStringValue();
            }

            if (!mode.IsNil())
            {
                url += "&rmode=" + mode.ToStringValue();
            }

            return Task.FromResult<FluidValue>(new StringValue(url));
        }
    }
}
