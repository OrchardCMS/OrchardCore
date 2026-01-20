using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata.Builders;

public abstract class ContentTypePartDefinitionBuilder
{
    protected readonly JsonObject _settings;

    public ContentTypePartDefinition Current { get; protected set; }
    public string Name { get; private set; }
    public string PartName { get; private set; }
    public string TypeName { get; private set; }

    protected ContentTypePartDefinitionBuilder(ContentTypePartDefinition part)
    {
        Current = part;
        Name = part.Name;
        PartName = part.PartDefinition.Name;
        TypeName = part.ContentTypeDefinition != null ? part.ContentTypeDefinition.Name : default;
        _settings = part.Settings?.Clone() ?? [];
    }

    public ContentTypePartDefinitionBuilder WithSettings<T>(T settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var jObject = JObject.FromObject(settings, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
        _settings[typeof(T).Name] = jObject;

        return this;
    }

    public ContentTypePartDefinitionBuilder MergeSettings(JsonObject settings)
    {
        _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);
        return this;
    }

    public ContentTypePartDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
    {
        var existingJObject = _settings[typeof(T).Name] as JsonObject;

        // If existing settings do not exist, create.
        if (existingJObject == null)
        {
            existingJObject = JObject.FromObject(new T(), ContentBuilderSettings.IgnoreDefaultValuesSerializer);
            _settings[typeof(T).Name] = existingJObject;
        }

        var settingsToMerge = existingJObject.ToObject<T>();
        setting(settingsToMerge);
        _settings[typeof(T).Name] = JObject.FromObject(settingsToMerge, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
        return this;
    }

    public abstract ContentTypePartDefinition Build();
}
