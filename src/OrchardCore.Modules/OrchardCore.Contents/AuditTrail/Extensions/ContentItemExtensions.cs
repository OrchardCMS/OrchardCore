using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.AuditTrail.Extensions
{
    public static class ContentItemExtensions
    {
        public static bool FindDiff(this ContentItem current, ContentItem previous, out JToken diff)
            => JObject.FromObject(current).FindDiff(JObject.FromObject(previous), out diff);
    }
}
