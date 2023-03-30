using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public abstract class ContentDefinition
    {
        public string Name { get; protected set; }

        private Dictionary<Type, object> NamedSettings = new Dictionary<Type, object>();

        /// <summary>
        /// Do not access this property directly. Migrate to use GetSettings and PopulateSettings.
        /// </summary>
        public JObject Settings { get; protected set; }

        public T GetSettings<T>() where T : new()
        {
            if (Settings == null)
            {
                return new T();
            }

            var namedSettings = NamedSettings;

            if (!namedSettings.TryGetValue(typeof(T), out var result))
            {
                var typeName = typeof(T).Name;

                JToken value;
                if (Settings.TryGetValue(typeName, out value))
                {
                    result = value.ToObject<T>();
                }
                else
                {
                    result = new T();
                }

                namedSettings = new Dictionary<Type, object>(NamedSettings);
                namedSettings[typeof(T)] = result;
                NamedSettings = namedSettings;
            }

            return (T)result;
        }

        public void PopulateSettings<T>(T target)
        {
            if (Settings == null)
            {
                return;
            }

            var typeName = typeof(T).Name;

            JToken value;
            if (Settings.TryGetValue(typeName, out value))
            {
                JsonConvert.PopulateObject(value.ToString(), target);
                NamedSettings = new Dictionary<Type, object>();
            }
        }
    }
}
