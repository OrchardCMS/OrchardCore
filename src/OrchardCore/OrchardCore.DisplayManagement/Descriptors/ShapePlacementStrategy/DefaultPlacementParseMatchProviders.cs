using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public class ContentTypePlacementParseMatchProvider : IPlacementParseMatchProvider
    {
        public string Key { get { return "ContentType"; } }

        public bool Match(ShapePlacementContext context, string expression)
        {
            var shape = context.ZoneShape as Shape;
            if (shape == null || shape.Properties["ContentItem"] == null)
            {
                return false;
            }

            var contentItem = (dynamic)shape.Properties["ContentItem"];

            if (expression.EndsWith("*"))
            {
                var prefix = expression.Substring(0, expression.Length - 1);
              
                return (contentItem?.ContentType ?? "").StartsWith(prefix);// || (context.Stereotype ?? "").StartsWith(prefix);
            }

            return contentItem.ContentType == expression;// || context.Stereotype == expression;
        }
    }
}
