using System;
using Fluid;
using Fluid.Values;
using Orchard.Liquid;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Media.Filters
{
    public class MediaFilters : ITemplateContextHandler
    {
        private readonly IServiceProvider _serviceProvider;

        private IMediaFileStore _mediaStore;

        public MediaFilters(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnTemplateProcessing(TemplateContext context)
        {
            context.Filters.AddFilter("media_url", (input, arguments, ctx) =>
            {
                _mediaStore = _mediaStore ?? _serviceProvider.GetService<IMediaFileStore>();

                var url = input.ToStringValue();
                var imageUrl = _mediaStore.GetPublicUrl(url);

                return new StringValue(imageUrl ?? url);
            });

            context.Filters.AddFilter("img_tag", (input, arguments, ctx) =>
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

                return new StringValue(imgTag) { Encode = false };
            });

            context.Filters.AddFilter("resize_url", (input, arguments, ctx) =>
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
                
                return new StringValue(url);
            });
        }
    }
}
