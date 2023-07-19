using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.ContentManagement.Display.Placement
{
    public class ContentPartPlacementNodeFilterProvider : ContentPlacementParseFilterProviderBase, IPlacementNodeFilterProvider
    {
        public string Key { get { return "contentPart"; } }

        public bool IsMatch(ShapePlacementContext context, JToken expression)
        {
            var contentItem = GetContent(context);
            if (contentItem == null)
            {
                return false;
            }

            if (expression is JArray)
            {
                return expression.Any(p => contentItem.Has(p.Value<string>()));
            }
            else
            {
                return contentItem.Has(expression.Value<string>());
            }
        }
    }

    public class ContentTypePlacementNodeFilterProvider : ContentPlacementParseFilterProviderBase, IPlacementNodeFilterProvider
    {
        public string Key { get { return "contentType"; } }

        public bool IsMatch(ShapePlacementContext context, JToken expression)
        {
            var contentItem = GetContent(context);
            if (contentItem == null)
            {
                return false;
            }

            IEnumerable<string> contentTypes;

            if (expression is JArray)
            {
                contentTypes = expression.Values<string>();
            }
            else
            {
                contentTypes = new string[] { expression.Value<string>() };
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
}
