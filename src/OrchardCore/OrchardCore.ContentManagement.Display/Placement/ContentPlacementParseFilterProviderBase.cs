using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.ContentManagement.Display.Placement
{
    public class ContentPlacementParseFilterProviderBase
    {
        protected static bool HasContent(ShapePlacementContext context)
        {
            return context.ZoneShape is Shape shape
                && shape.TryGetProperty("ContentItem", out object contentItem)
                && contentItem != null;
        }

        protected static ContentItem GetContent(ShapePlacementContext context)
        {
            if (HasContent(context) && context.ZoneShape is Shape shape && shape.TryGetProperty("ContentItem", out ContentItem contentItem))
            {
                return contentItem;
            }

            return null;
        }
    }
}
