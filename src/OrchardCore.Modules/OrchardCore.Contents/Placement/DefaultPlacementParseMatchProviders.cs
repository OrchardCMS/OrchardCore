using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Contents.Placement
{
    public enum MatchType
    {
        Any,
        All
    }

    public class ContentPartPlacementParseMatchOptions
    {
        [JsonProperty(PropertyName = "parts")]
        public IEnumerable<string> Parts { get; set; }

        [JsonProperty(PropertyName = "matchType")]
        public MatchType MatchType { get; set; }
    }

    public class ContentPartPlacementParseMatchProvider : ContentPlacementParseMatchProviderBase, IPlacementParseMatchProvider
    {
        public string Key { get { return "contentPart"; } }

        public bool Match(ShapePlacementContext context, JToken expression)
        {
            var contentItem = GetContent(context);
            if (contentItem == null)
            {
                return false;
            }

            var options = expression.ToObject<ContentPartPlacementParseMatchOptions>();

            return options.MatchType == MatchType.All ? options.Parts.All(p => contentItem.Has(p)) : options.Parts.Any(p => contentItem.Has(p));
        }
    }

    public class ContentTypePlacementParseMatchProvider : ContentPlacementParseMatchProviderBase, IPlacementParseMatchProvider
    {
        public string Key { get { return "contentType"; } }

        public bool Match(ShapePlacementContext context, JToken expression)
        {
            var contentItem = GetContent(context);
            if (contentItem == null)
            {
                return false;
            }

            var contentTypes = expression.ToObject<IEnumerable<string>>();

            return contentTypes.Any(ct =>
            {
                if (ct.EndsWith("*"))
                {
                    var prefix = ct.Substring(0, ct.Length - 1);

                    return (contentItem?.ContentType ?? "").StartsWith(prefix);// || (context.Stereotype ?? "").StartsWith(prefix);
                }

                return contentItem.ContentType == ct;// || context.Stereotype == expression;
            });
        }
    }

    public class ContentPlacementParseMatchProviderBase
    {
        protected bool HasContent(ShapePlacementContext context)
        {
            var shape = context.ZoneShape as Shape;
            return shape != null && shape.Properties["ContentItem"] != null;
        }

        protected ContentItem GetContent(ShapePlacementContext context)
        {
            if (!HasContent(context))
            {
                return null;
            }

            var shape = context.ZoneShape as Shape;
            return shape.Properties["ContentItem"] as ContentItem;
        }
    }
}
