using System;
using Fluid;
using Fluid.Values;
using Orchard.ContentManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Liquid.Filters
{
    public class ContentFilters : ITemplateContextHandler
    {
        private IContentManager _contentManager;
        private ISlugService _slugService;

        private readonly IServiceProvider _serviceProvider;

        public ContentFilters(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnTemplateProcessing(TemplateContext context)
        {
            context.Filters.AddFilter("date", (input, arguments, ctx) =>
            {
                var format = arguments.At(0).ToStringValue();

                switch(input.ToObjectValue())
                {
                    case DateTime dateTime:
                        return new StringValue(dateTime.ToString(format));

                    case DateTimeOffset dateTimeOffset:
                        return new StringValue(dateTimeOffset.ToString(format));

                    default:
                        return NilValue.Instance;
                }
            });

            context.Filters.AddFilter("slugify", (input, arguments, ctx) =>
            {
                var text = input.ToStringValue();
                _slugService = _slugService ?? _serviceProvider.GetRequiredService<ISlugService>();

                return new StringValue(_slugService.Slugify(text));
            });

            context.Filters.AddAsyncFilter("container", async (input, arguments, ctx) =>
            {
                var contentItem = input.ToObjectValue() as ContentItem;

                if (contentItem == null)
                {
                    throw new ArgumentException("A Content Item was expected");
                }

                _contentManager = _contentManager ?? _serviceProvider.GetRequiredService<IContentManager>();
                
                string containerId = contentItem.Content?.ContainedPart?.ListContentItemId;

                if (containerId != null)
                {
                    var container = await _contentManager.GetAsync(containerId);

                    if (container != null)
                    {
                        return new ObjectValue(container);
                    }
                }

                return new ObjectValue(contentItem);
            });
        }
    }
}
