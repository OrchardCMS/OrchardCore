using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.ContentManagement.Display.Placement;

public class ContentPlacementParseFilterProviderBase
{
    protected static bool HasContent(ShapePlacementContext context)
    {
        var shape = context.ZoneShape as Shape;
        return shape != null && shape.TryGetProperty("ContentItem", out object contentItem) && contentItem != null;
    }

    protected static ContentItem GetContent(ShapePlacementContext context)
    {
        if (!HasContent(context))
        {
            return null;
        }

        var shape = context.ZoneShape as Shape;
        shape.TryGetProperty("ContentItem", out ContentItem contentItem);

        return contentItem;
    }
}
