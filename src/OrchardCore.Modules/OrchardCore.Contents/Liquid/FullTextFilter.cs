using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid;

public class FullTextFilter : ILiquidFilter
{
    private readonly IContentManager _contentManager;
    private readonly IContentItemRecursionHelper<FullTextFilter> _fullTextRecursionHelper;

    public FullTextFilter(
        IContentManager contentManager,
        IContentItemRecursionHelper<FullTextFilter> fullTextRecursionHelper)
    {
        _contentManager = contentManager;
        _fullTextRecursionHelper = fullTextRecursionHelper;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        if (input.Type == FluidValues.Array)
        {
            var contentItems = new List<ContentItem>();
            await foreach (var objValue in input.EnumerateAsync(ctx))
            {
                var contentItem = GetContentItem(objValue, ctx);
                if (contentItem != null)
                {
                    if (!_fullTextRecursionHelper.IsRecursive(contentItem))
                    {
                        contentItems.Add(contentItem);
                    }
                }
            }

            if (contentItems.Count == 0)
            {
                return NilValue.Instance;
            }

            var aspects = new List<FullTextAspect>();

            foreach (var contentItem in contentItems)
            {
                aspects.Add(await _contentManager.PopulateAspectAsync<FullTextAspect>(contentItem));
            }

            // When returning segments separate them so multiple segments are indexed individually.
            return ArrayValue.Create(aspects.SelectMany(x => x.Segments).Where(x => !string.IsNullOrEmpty(x)).Select(x => StringValue.Create(x + ' ')).ToArray());
        }
        else
        {
            var contentItem = GetContentItem(input, ctx);

            if (contentItem == null || _fullTextRecursionHelper.IsRecursive(contentItem))
            {
                return NilValue.Instance;
            }

            var fullTextAspect = await _contentManager.PopulateAspectAsync<FullTextAspect>(contentItem);

            // Remove empty strings as display text is often unused in contained content items.
            return ArrayValue.Create(fullTextAspect.Segments.Where(x => !string.IsNullOrEmpty(x)).Select(x => StringValue.Create(x)).ToArray());
        }
    }

    private static ContentItem GetContentItem(FluidValue input, LiquidTemplateContext ctx)
    {
        var obj = input.ToObjectValue(ctx);

        if (obj is not ContentItem contentItem)
        {
            contentItem = null;

            if (obj is JsonObject jObject)
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
