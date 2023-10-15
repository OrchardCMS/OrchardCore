using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public abstract class ContentDefinition
    {
        public string Name { get; protected set; }

        private Dictionary<Type, object> _namedSettings = new();

        /// <summary>
        /// Do not access this property directly. Migrate to use GetSettings and PopulateSettings.
        /// </summary>
        public JsonObject Settings { get; protected set; }

        public T GetSettings<T>() where T : new()
        {
            if (Settings == null)
            {
                return new T();
            }

            var namedSettings = _namedSettings;

            if (!namedSettings.TryGetValue(typeof(T), out var result))
            {
                var typeName = typeof(T).Name;

                result = Settings.TryGetPropertyValue(typeName, out var value)
                    ? value.Deserialize<T>()
                    : new T();

                namedSettings = new Dictionary<Type, object>(_namedSettings)
                {
                    [typeof(T)] = result,
                };

                _namedSettings = namedSettings;
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

            if (Settings.TryGetPropertyValue(typeName, out var value) && value != null)
            {
                // TODO https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/migrate-from-newtonsoft?pivots=dotnet-7-0#populate-existing-objects
                JsonConvert.PopulateObject(value.ToString(), target);
                _namedSettings = new Dictionary<Type, object>();
            }
        }
    }
}
