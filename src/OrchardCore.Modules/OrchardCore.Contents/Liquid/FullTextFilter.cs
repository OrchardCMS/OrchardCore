using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class FullTextFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'full_text_aspect'");
            }

            var serviceProvider = (IServiceProvider)services;

            var contentManager = serviceProvider.GetRequiredService<IContentManager>();
            var fullTextRecursionHelper = serviceProvider.GetRequiredService<IContentItemRecursionHelper<FullTextFilter>>();

            if (input.Type == FluidValues.Array)
            {
                var contentItems = new List<ContentItem>();
                foreach(var objValue in input.Enumerate())
                {
                    var contentItem = GetContentItem(objValue);
                    if (contentItem != null)
                    {
                        if (!fullTextRecursionHelper.IsRecursive(contentItem))
                        {
                            contentItems.Add(contentItem);
                        }
                    }
                }

                if (!contentItems.Any())
                {
                    return NilValue.Instance;
                }

                var aspects = new List<FullTextAspect>();

                foreach(var contentItem in contentItems)
                {
                    aspects.Add(await contentManager.PopulateAspectAsync<FullTextAspect>(contentItem));
                }

                // When returning segments seperate them so multiple segments are indexed individually.
                return new ArrayValue(aspects.SelectMany(x => x.Segments).Where(x => !String.IsNullOrEmpty(x)).Select(x => new StringValue(x + ' ')));
            }
            else
            {
                var contentItem = GetContentItem(input);

                if (contentItem == null || fullTextRecursionHelper.IsRecursive(contentItem))
                {
                    return NilValue.Instance;
                }

                var fullTextAspect = await contentManager.PopulateAspectAsync<FullTextAspect>(contentItem);

                // Remove empty strings as display text is often unused in contained content items.
                return new ArrayValue(fullTextAspect.Segments.Where(x => !String.IsNullOrEmpty(x)).Select(x => new StringValue(x)));
            }
        }

        private static ContentItem GetContentItem(FluidValue input)
        {
            var obj = input.ToObjectValue();

            if (!(obj is ContentItem contentItem))
            {
                contentItem = null;

                if (obj is JObject jObject)
                {
                    contentItem = jObject.ToObject<ContentItem>();
                    // If input is a 'JObject' but which not represents a 'ContentItem',
                    // a 'ContentItem' is still created but with some null properties.
                    if (contentItem?.ContentItemId == null)
                    {
                        return null;
                    }
                }
            }

            return contentItem;
        }
    }
}
