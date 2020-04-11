using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Media.Filters
{
    public class MediaUrlFilter : ILiquidFilter
    {
        private readonly IMediaFileStore _mediaFileStore;

        public MediaUrlFilter(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();
            var imageUrl = _mediaFileStore.MapPathToPublicUrl(url);

            return new ValueTask<FluidValue>(new StringValue(imageUrl ?? url));
        }
    }

    public class ImageTagFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
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

    public class ResizeUrlFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();

            var queryStringParams = new Dictionary<string, string>();

            var width = arguments["width"].Or(arguments.At(0));
            var height = arguments["height"].Or(arguments.At(1));
            var mode = arguments["mode"].Or(arguments.At(2));

            if (!width.IsNil())
            {
                queryStringParams.Add("width", width.ToStringValue());
            }

            if (!height.IsNil())
            {
                queryStringParams.Add("height", height.ToStringValue());
            }

            if (!mode.IsNil())
            {
                queryStringParams.Add("rmode", mode.ToStringValue());
            }

            return new ValueTask<FluidValue>(new StringValue(QueryHelpers.AddQueryString(url, queryStringParams)));
        }
    }
}
