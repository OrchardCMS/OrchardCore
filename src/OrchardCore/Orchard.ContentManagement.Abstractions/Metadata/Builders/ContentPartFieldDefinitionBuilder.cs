using System;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentManagement.Metadata.Builders
{
    public abstract class ContentPartFieldDefinitionBuilder
    {
        protected readonly JObject _settings;

        public ContentPartFieldDefinition Current { get; private set; }
        public abstract string Name { get; }
        public abstract string FieldType { get; }
        public abstract string PartName { get; }

        protected ContentPartFieldDefinitionBuilder(ContentPartFieldDefinition field)
        {
            Current = field;

            _settings = new JObject(field.Settings);
        }

        public ContentPartFieldDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public ContentPartFieldDefinitionBuilder MergeSettings(JObject settings)
        {
            _settings.Merge(settings, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            return this;
        }

        public ContentPartFieldDefinitionBuilder MergeSettings(object model)
        {
            _settings.Merge(JObject.FromObject(model), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            return this;
        }

        public ContentPartFieldDefinitionBuilder WithSettings<T>(T settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var jObject = JObject.FromObject(settings);
            _settings[typeof(T).Name] = jObject;

            return this;
        }

        public abstract ContentPartFieldDefinitionBuilder OfType(ContentFieldDefinition fieldDefinition);
        public abstract ContentPartFieldDefinitionBuilder OfType(string fieldType);
        public abstract ContentPartFieldDefinition Build();
    }
}