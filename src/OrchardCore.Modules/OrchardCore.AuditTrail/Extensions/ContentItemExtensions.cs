using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class ContentItemExtensions
    {
        public static DiffNode LogPropertyChange(
            this ContentItem currentContentItem,
            ContentItem previousContentItem, string property)
        {
            var currentContentItemProperty = JObject.FromObject(currentContentItem).Property(property);
            var previousContentItemProperty = JObject.FromObject(previousContentItem).Property(property);

            if (currentContentItemProperty.Value.ToString() != previousContentItemProperty.Value.ToString())
            {
                return new DiffNode
                {
                    Context = currentContentItem.ContentType + "/" + property,
                    Current = currentContentItemProperty.Value,
                    Previous = previousContentItemProperty.Value,
                    Type = DiffType.Change
                };
            }

            return null;
        }
    }
}
