using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata.Builders;

public abstract class ContentPartFieldDefinitionBuilder
{
    protected readonly JsonObject _settings;

    public ContentPartFieldDefinition Current { get; private set; }
    public abstract string Name { get; }
    public abstract string FieldType { get; }
    public abstract string PartName { get; }

    protected ContentPartFieldDefinitionBuilder(ContentPartFieldDefinition field)
    {
        Current = field;

        _settings = field.Settings.Clone();
    }

    public ContentPartFieldDefinitionBuilder MergeSettings(JsonObject settings)
    {
        _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);
        return this;
    }

    public ContentPartFieldDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
    {
        var existingJObject = _settings[typeof(T).Name] as JsonObject;
        // If existing settings do not exist, create.
        if (existingJObject == null)
        {
            existingJObject = ToJsonObject(new T());
            _settings[typeof(T).Name] = existingJObject;
        }

        var settingsToMerge = existingJObject.ToObject<T>();
        setting(settingsToMerge);
        _settings[typeof(T).Name] = ToJsonObject(settingsToMerge);
        return this;
    }

    public ContentPartFieldDefinitionBuilder WithSettings<T>(T settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _settings[typeof(T).Name] = ToJsonObject(settings);

        return this;
    }

    public ContentPartFieldDefinitionBuilder WithSettings<T>() where T : class, new()
    {
        _settings[typeof(T).Name] = ToJsonObject(new T());

        return this;
    }

    public abstract ContentPartFieldDefinitionBuilder OfType(ContentFieldDefinition fieldDefinition);
    public abstract ContentPartFieldDefinitionBuilder OfType(string fieldType);
    public abstract ContentPartFieldDefinition Build();

    private static JsonObject ToJsonObject(object obj)
        => JObject.FromObject(obj, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
}
