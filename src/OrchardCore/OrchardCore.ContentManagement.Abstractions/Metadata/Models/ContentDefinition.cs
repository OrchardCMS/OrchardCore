using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public abstract class ContentDefinition
    {
        public string Name { get; protected set; }

        /// <summary>
        /// Do not access this property directly. Migrate to use GetSettings and PopulateSettings.
        /// </summary>
        public JObject Settings { get; protected set; }

        public T GetSettings<T>() where T : new()
        {
            var typeName = typeof(T).Name;

            if (Settings == null)
            {
                return new T();
            }

            JToken value;
            if (Settings.TryGetValue(typeName, out value))
            {
                return value.ToObject<T>();
            }

            return new T();
        }

        public void PopulateSettings<T>(T target)
        {
            var typeName = typeof(T).Name;

            if (Settings == null)
            {
                return;
            }

            JToken value;
            if (Settings.TryGetValue(typeName, out value))
            {
                JsonConvert.PopulateObject(value.ToString(), target);
            }
        }
    }
}
