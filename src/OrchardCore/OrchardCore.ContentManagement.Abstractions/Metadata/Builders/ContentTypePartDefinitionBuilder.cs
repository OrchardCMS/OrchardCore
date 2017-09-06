using System;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata.Builders
{
    public abstract class ContentTypePartDefinitionBuilder
    {
        protected readonly JObject _settings;

        public ContentTypePartDefinition Current { get; protected set; }
        public string Name { get; private set; }
        public string PartName { get; private set; }
        public string TypeName { get; private set; }

        protected ContentTypePartDefinitionBuilder(ContentTypePartDefinition part)
        {
            Current = part;
            Name = part.Name;
            PartName = part.PartDefinition.Name;
            TypeName = part.ContentTypeDefinition != null ? part.ContentTypeDefinition.Name : default(string);
            _settings = new JObject(part.Settings);
        }

        public ContentTypePartDefinitionBuilder WithSettings<T>(T settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var jObject = JObject.FromObject(settings, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
            _settings[typeof(T).Name] = jObject;

            return this;
        }

        public ContentTypePartDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public ContentTypePartDefinitionBuilder WithSetting(string name, string[] values)
        {
            _settings[name] = new JArray(values);
            return this;
        }

        public ContentTypePartDefinitionBuilder MergeSettings(JObject settings)
        {
            _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);
            return this;
        }

        public abstract ContentTypePartDefinition Build();
    }
}