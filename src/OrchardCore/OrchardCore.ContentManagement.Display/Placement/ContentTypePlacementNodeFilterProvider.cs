using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.ContentManagement.Display.Placement
{
    public class ContentTypePlacementNodeFilterProvider : ContentPlacementParseFilterProviderBase, IPlacementNodeFilterProvider
    {
        public string Key
        {
            get
            {
                return "contentType";
            }
        }

        public bool IsMatch(ShapePlacementContext context, JToken expression)
        {
            var contentItem = GetContent(context);

            if (contentItem?.ContentType == null)
            {
                return false;
            }

            var contentTypes = GetContentTypes(expression);

            return contentTypes.Any(ct =>
            {
                if (ct.EndsWith('*'))
                {
                    var prefix = ct.Substring(0, ct.Length - 1);

                    return contentItem.ContentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    || (GetStereotype(context) ?? String.Empty).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                }

                return contentItem.ContentType == ct || GetStereotype(context) == ct;
            });
        }

        private static IEnumerable<string> GetContentTypes(JToken expression)
        {
            if (expression is JArray)
            {
                return expression.Values<string>();
            }

            return new string[] { expression.Value<string>() };
        }

        private static string GetStereotype(ShapePlacementContext context)
        {
            if (context.ZoneShape is Shape shape && shape.Properties.TryGetValue("Stereotype", out var stereotypeVal))
            {
                return stereotypeVal?.ToString();
            }

            return null;
        }
    }
}
