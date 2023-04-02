using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.ContentManagement.Display.Placement
{
    public class ContentPartPlacementNodeFilterProvider : ContentPlacementParseFilterProviderBase, IPlacementNodeFilterProvider
    {
        public string Key
        {
            get
            {
                return "contentPart";
            }
        }

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

            return contentItem.Has(expression.Value<string>());
        }
    }
}
