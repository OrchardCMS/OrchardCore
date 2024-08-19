using System.Text.Json.Nodes;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.ContentManagement.Display.Placement;

public class ContentPartPlacementNodeFilterProvider : ContentPlacementParseFilterProviderBase, IPlacementNodeFilterProvider
{
    public string Key { get { return "contentPart"; } }

    public bool IsMatch(ShapePlacementContext context, object expression)
    {
        var contentItem = GetContent(context);
        if (contentItem is null)
        {
            return false;
        }

        var jsonNode = JNode.FromObject(expression);
        if (jsonNode is JsonArray jsonArray)
        {
            return jsonArray.Any(p => contentItem.Has(p.Value<string>()));
        }
        else
        {
            return contentItem.Has(jsonNode.Value<string>());
        }
    }
}

public class ContentTypePlacementNodeFilterProvider : ContentPlacementParseFilterProviderBase, IPlacementNodeFilterProvider
{
    public string Key { get { return "contentType"; } }

    public bool IsMatch(ShapePlacementContext context, object expression)
    {
        var contentItem = GetContent(context);
        if (contentItem is null)
        {
            return false;
        }

        IEnumerable<string> contentTypes;

        var jsonNode = JNode.FromObject(expression);
        if (jsonNode is JsonArray jsonArray)
        {
            contentTypes = jsonArray.Values<string>();
        }
        else
        {
            contentTypes = new string[] { jsonNode.Value<string>() };
        }

        return contentTypes.Any(ct =>
        {
            if (ct.EndsWith('*'))
            {
                var prefix = ct[..^1];

                return (contentItem.ContentType ?? "").StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || (GetStereotype(context) ?? "").StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            return contentItem.ContentType == ct || GetStereotype(context) == ct;
        });
    }

    private static string GetStereotype(ShapePlacementContext context)
    {
        var shape = context.ZoneShape as Shape;
        object stereotypeVal = null;
        shape?.Properties?.TryGetValue("Stereotype", out stereotypeVal);
        return stereotypeVal?.ToString();
    }
}

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
