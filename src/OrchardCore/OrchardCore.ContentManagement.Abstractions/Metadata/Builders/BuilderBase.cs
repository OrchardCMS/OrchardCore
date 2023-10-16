using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Builders;

public abstract class BuilderBase
{
    protected readonly JsonObject _settings;

    protected BuilderBase(JsonObject settings)
    {
        _settings = settings == null ? new JsonObject() : new JsonObject(settings);
    }

    protected void WithSettingImpl(string name, object value)
    {
        _settings[name] = JsonSerializer.SerializeToNode(value);
    }

    protected void MergeSettingsImpl(JsonObject settings)
    {
        _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);
    }

    protected void MergeSettingsImpl(object model)
    {
        _settings.Merge(JsonSerializer.SerializeToNode(model), ContentBuilderSettings.JsonMergeSettings);
    }

    protected void MergeSettingsImpl<T>(Action<T> setting) where T : class, new()
    {
        // If existing settings do not exist, create.
        var existingJObject = _settings[typeof(T).Name] ?? AddSettings(new T());

        var settingsToMerge = existingJObject.Deserialize<T>();
        setting(settingsToMerge);
        AddSettings(settingsToMerge);
    }

    protected void WithSettingsImpl<T>(T settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        AddSettings(settings);
    }

    private JsonNode AddSettings<T>(T value)
    {
        var jObject = JsonSerializer.SerializeToNode(value, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
        _settings[typeof(T).Name] = jObject;
        return jObject;
    }
}
