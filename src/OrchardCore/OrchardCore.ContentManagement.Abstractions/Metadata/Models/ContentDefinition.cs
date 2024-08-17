using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Models;

public abstract class ContentDefinition
{
    public string Name { get; protected set; }

    private Dictionary<Type, object> _namedSettings = [];

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

            JsonNode value;
            if (Settings.TryGetPropertyValue(typeName, out value))
            {
                result = value.ToObject<T>();
            }
            else
            {
                result = new T();
            }

            namedSettings = new Dictionary<Type, object>(_namedSettings)
            {
                [typeof(T)] = result,
            };

            _namedSettings = namedSettings;
        }

        return (T)result;
    }
}
