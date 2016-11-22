using Newtonsoft.Json.Linq;

namespace Orchard.Settings
{
    public static class SiteSettingsExtensions
    {
        public static T As<T>(this ISite site)
        {
            JToken value;
            var typeName = typeof(T).Name;

            if (site.Properties.TryGetValue(typeName, out value))
            {
                return value.ToObject<T>();
            }

            return default(T);
        }
    }
}
