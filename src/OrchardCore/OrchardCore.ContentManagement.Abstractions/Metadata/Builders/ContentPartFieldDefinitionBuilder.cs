using System;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata.Builders
{
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

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentPartFieldDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentPartFieldDefinitionBuilder WithSetting(string name, string[] values)
        {
            _settings[name] = JArray.FromObject(values);
            return this;
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
                existingJObject = JObject.FromObject(new T(), ContentBuilderSettings.IgnoreDefaultValuesSerializer);
                _settings[typeof(T).Name] = existingJObject;
            }

            var settingsToMerge = existingJObject.ToObject<T>();
            setting(settingsToMerge);
            _settings[typeof(T).Name] = JObject.FromObject(settingsToMerge, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
            return this;
        }

        public ContentPartFieldDefinitionBuilder WithSettings<T>(T settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var jObject = JObject.FromObject(settings, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
            _settings[typeof(T).Name] = jObject;

            return this;
        }

        public abstract ContentPartFieldDefinitionBuilder OfType(ContentFieldDefinition fieldDefinition);
        public abstract ContentPartFieldDefinitionBuilder OfType(string fieldType);
        public abstract ContentPartFieldDefinition Build();
    }
}
