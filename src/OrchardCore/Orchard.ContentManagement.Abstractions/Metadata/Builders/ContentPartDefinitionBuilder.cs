using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Metadata.Models;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.Metadata.Builders
{
    public class ContentPartDefinitionBuilder
    {
        private readonly ContentPartDefinition _part;
        private string _name;
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
                _name = existing.Name;
                _fields = existing.Fields.ToList();
                _settings = new JObject(existing.Settings);
            }
        }

        public string Name { get { return _name; } }

        public ContentPartDefinition Build()
        {
            return new ContentPartDefinition(_name, _fields, _settings);
        }

        public ContentPartDefinitionBuilder Named(string name)
        {
            _name = name;
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

        public ContentPartDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public ContentPartDefinitionBuilder MergeSettings(JObject settings)
        {
            _settings.Merge(settings, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            return this;
        }

        public ContentPartDefinitionBuilder WithSettings<T>(T settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var jObject = JObject.FromObject(settings);
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

        class FieldConfigurerImpl : ContentPartFieldDefinitionBuilder
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