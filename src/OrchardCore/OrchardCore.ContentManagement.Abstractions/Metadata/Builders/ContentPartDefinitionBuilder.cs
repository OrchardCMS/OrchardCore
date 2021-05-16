using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentManagement.Metadata.Builders
{
    public class ContentPartDefinitionBuilder
    {
        private readonly ContentPartDefinition _part;
        private readonly IList<ContentPartFieldDefinition> _fields;
        private readonly JObject _settings;

        public ContentPartDefinition Current { get; private set; }

        public ContentPartDefinitionBuilder()
            : this(new ContentPartDefinition(null))
        {
        }

        public ContentPartDefinitionBuilder(ContentPartDefinition existing)
        {
            _part = existing;

            if (existing == null)
            {
                _fields = new List<ContentPartFieldDefinition>();
                _settings = new JObject();
            }
            else
            {
                Name = existing.Name;
                _fields = existing.Fields.ToList();
                _settings = new JObject(existing.Settings);
            }
        }

        public string Name { get; private set; }

        public ContentPartDefinition Build()
        {
            if (!Name[0].IsLetter())
            {
                throw new ArgumentException("Content part name must start with a letter", "name");
            }
            if (!String.Equals(Name, Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Content part name contains invalid characters", "name");
            }
            if (Name.IsReservedContentName())
            {
                throw new ArgumentException("Content part name is reserved for internal use", "name");
            }

            return new ContentPartDefinition(Name, _fields, _settings);
        }

        public ContentPartDefinitionBuilder Named(string name)
        {
            Name = name;
            return this;
        }

        public ContentPartDefinitionBuilder RemoveField(string fieldName)
        {
            var existingField = _fields.SingleOrDefault(x => x.Name == fieldName);
            if (existingField != null)
            {
                _fields.Remove(existingField);
            }
            return this;
        }

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentPartDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public ContentPartDefinitionBuilder MergeSettings(JObject settings)
        {
            _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);
            return this;
        }

        public ContentPartDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
        {
            var existingJObject = _settings[typeof(T).Name] as JObject;
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

        public ContentPartDefinitionBuilder WithSettings<T>(T settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var jObject = JObject.FromObject(settings, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
            _settings[typeof(T).Name] = jObject;

            return this;
        }

        public ContentPartDefinitionBuilder WithField(string fieldName)
        {
            return WithField(fieldName, configuration => { });
        }

        public ContentPartDefinitionBuilder WithField(string fieldName, Action<ContentPartFieldDefinitionBuilder> configuration)
        {
            var existingField = _fields.FirstOrDefault(x => x.Name == fieldName);
            if (existingField != null)
            {
                var toRemove = _fields.Where(x => x.Name == fieldName).ToArray();
                foreach (var remove in toRemove)
                {
                    _fields.Remove(remove);
                }
            }
            else
            {
                existingField = new ContentPartFieldDefinition(null, fieldName, new JObject());
            }
            var configurer = new FieldConfigurerImpl(existingField, _part);
            configuration(configurer);
            _fields.Add(configurer.Build());
            return this;
        }

        public async Task<ContentPartDefinitionBuilder> WithFieldAsync(string fieldName, Func<ContentPartFieldDefinitionBuilder, Task> configurationAsync)
        {
            var existingField = _fields.FirstOrDefault(x => x.Name == fieldName);

            if (existingField != null)
            {
                var toRemove = _fields.Where(x => x.Name == fieldName).ToArray();
                foreach (var remove in toRemove)
                {
                    _fields.Remove(remove);
                }
            }
            else
            {
                existingField = new ContentPartFieldDefinition(null, fieldName, new JObject());
            }

            var configurer = new FieldConfigurerImpl(existingField, _part);

            await configurationAsync(configurer);

            _fields.Add(configurer.Build());

            return this;
        }

        private class FieldConfigurerImpl : ContentPartFieldDefinitionBuilder
        {
            private ContentFieldDefinition _fieldDefinition;
            private readonly ContentPartDefinition _partDefinition;
            private readonly string _fieldName;

            public FieldConfigurerImpl(ContentPartFieldDefinition field, ContentPartDefinition part)
                : base(field)
            {
                _fieldDefinition = field.FieldDefinition;
                _fieldName = field.Name;
                _partDefinition = part;
            }

            public override ContentPartFieldDefinition Build()
            {
                if (!_fieldName[0].IsLetter())
                {
                    throw new ArgumentException("Content field name must start with a letter", "name");
                }
                if (!String.Equals(_fieldName, _fieldName.ToSafeName(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Content field name contains invalid characters", "name");
                }

                return new ContentPartFieldDefinition(_fieldDefinition, _fieldName, _settings);
            }

            public override string Name
            {
                get { return _fieldName; }
            }

            public override string FieldType
            {
                get { return _fieldDefinition.Name; }
            }

            public override string PartName
            {
                get { return _partDefinition.Name; }
            }

            public override ContentPartFieldDefinitionBuilder OfType(ContentFieldDefinition fieldDefinition)
            {
                _fieldDefinition = fieldDefinition;
                return this;
            }

            public override ContentPartFieldDefinitionBuilder OfType(string fieldType)
            {
                _fieldDefinition = new ContentFieldDefinition(fieldType);
                return this;
            }
        }
    }
}
