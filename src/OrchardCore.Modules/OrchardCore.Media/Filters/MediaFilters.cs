using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
                imgTag += $" {name.Replace("_", "-")}=\"{arguments[name].ToStringValue()}\"";
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

            if (!url.Contains("?"))
            {
                url += "?";
            }

            var width = arguments["width"].Or(arguments.At(0));
            var height = arguments["height"].Or(arguments.At(1));
            var mode = arguments["mode"].Or(arguments.At(2));

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

            return new ValueTask<FluidValue>(new StringValue(url));
        }
    }

    public class MediaAppendVersionFilter : ILiquidFilter
    {
        private readonly IFileVersionProvider _fileVersionProvider;
        private string _pathBase;

        public MediaAppendVersionFilter(IFileVersionProvider fileVersionProvider)
        {
            _fileVersionProvider = fileVersionProvider;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();

            if (String.IsNullOrEmpty(_pathBase))
            {
                if (!ctx.AmbientValues.TryGetValue("ViewContext", out var viewContext))
                {
                    throw new ArgumentException("ViewContext missing while invoking 'append_version'");
                }
                _pathBase = String.Concat(((ViewContext)viewContext).HttpContext.Request.PathBase.ToString(), Startup.AssetsUrlPrefix);
            }
            var imageUrl = _fileVersionProvider.AddFileVersionToPath(_pathBase, url);

            return new ValueTask<FluidValue>(new StringValue(imageUrl ?? url));
        }
    }
}
