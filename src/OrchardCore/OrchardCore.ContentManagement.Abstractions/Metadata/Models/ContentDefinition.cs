using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public abstract class ContentDefinition
    {
        public string Name { get; protected set; }

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
    }
}
