using Newtonsoft.Json.Linq;
using OrchardCore.Entities;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class EntityExtensions
    {
        public static JToken Get(this IEntity entity, string name) => entity.Properties[name] ?? null;
    }
}
