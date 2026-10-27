using System.Runtime.CompilerServices;
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

        var contentParts = GetValues(expression);

        for (var i = 0; i < contentParts.Length; i++)
        {
            if (contentItem.Has(contentParts[i]))
            {
                return true;
            }
        }

        return false;
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

        var contentTypes = GetValues(expression);

        for (var i = 0; i < contentTypes.Length; i++)
        {
            var ct = contentTypes[i];

            if (ct.EndsWith('*'))
            {
                var prefix = ct.AsSpan(0, ct.Length - 1);

                if ((contentItem.ContentType ?? "").AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                    (GetStereotype(context) ?? "").AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (contentItem.ContentType == ct || GetStereotype(context) == ct)
            {
                return true;
            }
        }

        return false;
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
    // The filter expressions come from the placement files and live as long as the shape tables, so they
    // are parsed once instead of being converted to JSON on each placement evaluation.
    private static readonly ConditionalWeakTable<object, string[]> s_values = [];

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

    /// <summary>
    /// Gets the string values of a filter expression, which can be a single value or an array of values.
    /// </summary>
    protected static string[] GetValues(object expression)
    {
        if (expression is null)
        {
            return [];
        }

        if (expression is string value)
        {
            return [value];
        }

        return s_values.GetValue(expression, static expression => ParseValues(expression));
    }

    private static string[] ParseValues(object expression)
    {
        if (expression is IEnumerable<string> values)
        {
            return values.Where(v => v != null).ToArray();
        }

        var jsonNode = JNode.FromObject(expression);

        if (jsonNode is JsonArray jsonArray)
        {
            return jsonArray.Values<string>().Where(v => v != null).ToArray();
        }

        var jsonValue = jsonNode?.Value<string>();

        return jsonValue is null ? [] : [jsonValue];
    }
}
