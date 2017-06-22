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

        public MediaFilters(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnTemplateProcessing(TemplateContext context)
        {
            context.Filters.AddFilter("media_url", (input, arguments, ctx) =>
            {
                var mediaStore = _serviceProvider.GetService<IMediaFileStore>();

                var url = input.ToStringValue();
                var imageUrl = mediaStore.GetPublicUrl(url);

                return new StringValue(imageUrl ?? url);
            });

            context.Filters.AddFilter("img_tag", (input, arguments, ctx) =>
            {
                var url = input.ToStringValue();
                var alt = arguments.At(0).ToStringValue();

                return new StringValue($"<img src=\"{url}\" alt=\"{alt}\" />") { Encode = false };
            });
        }
    }
}
