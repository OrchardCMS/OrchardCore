using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentManagement.Metadata.Builders
{
    public class ContentTypeDefinitionBuilder
    {
        private string _name;
        private string _displayName;
        private readonly IList<ContentTypePartDefinition> _parts;
        private readonly JObject _settings;

        public ContentTypeDefinition Current { get; private set; }

        public ContentTypeDefinitionBuilder()
            : this(new ContentTypeDefinition(null, null))
        {
        }

        public ContentTypeDefinitionBuilder(ContentTypeDefinition existing)
        {
            Current = existing;

            if (existing == null)
            {
                _parts = new List<ContentTypePartDefinition>();
                _settings = new JObject();
            }
            else
            {
                _name = existing.Name;
                _displayName = existing.DisplayName;
                _parts = existing.Parts.ToList();
                _settings = new JObject(existing.Settings);
            }
        }

        public ContentTypeDefinition Build()
        {
            return new ContentTypeDefinition(_name, _displayName, _parts, _settings);
        }

        public ContentTypeDefinitionBuilder Named(string name)
        {
            _name = name;
            return this;
        }

        public ContentTypeDefinitionBuilder DisplayedAs(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        public ContentTypeDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public ContentTypeDefinitionBuilder MergeSettings(JObject settings)
        {
            _settings.Merge(settings, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            return this;
        }

        public ContentTypeDefinitionBuilder WithSettings<T>(T settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var jObject = JObject.FromObject(settings);
            _settings[typeof(T).Name] = jObject;

            return this;
        }

        public ContentTypeDefinitionBuilder RemovePart(string partName)
        {
            var existingPart = _parts.SingleOrDefault(x => x.Name == partName);
            if (existingPart != null)
            {
                _parts.Remove(existingPart);
            }
            return this;
        }

        public ContentTypeDefinitionBuilder WithPart(string partName)
        {
            return WithPart(partName, configuration => { });
        }

        public ContentTypeDefinitionBuilder WithPart(string name, string partName)
        {
            return WithPart(name, new ContentPartDefinition(partName), configuration => { });
        }

        public ContentTypeDefinitionBuilder WithPart(string name, string partName, Action<ContentTypePartDefinitionBuilder> configuration)
        {
            return WithPart(name, new ContentPartDefinition(partName), configuration);
        }

        public ContentTypeDefinitionBuilder WithPart(string partName, Action<ContentTypePartDefinitionBuilder> configuration)
        {
            return WithPart(partName, new ContentPartDefinition(partName), configuration);
        }

        public ContentTypeDefinitionBuilder WithPart(string name, ContentPartDefinition partDefinition, Action<ContentTypePartDefinitionBuilder> configuration)
        {
            var existingPart = _parts.FirstOrDefault(x => x.Name == name );
            if (existingPart != null)
            {
                _parts.Remove(existingPart);
            }
            else
            {
                existingPart = new ContentTypePartDefinition(name, partDefinition, new JObject());
                existingPart.ContentTypeDefinition = Current;
            }

            var configurer = new PartConfigurerImpl(existingPart);
            configuration(configurer);
            _parts.Add(configurer.Build());
            return this;
        }

        class PartConfigurerImpl : ContentTypePartDefinitionBuilder
        {
            private readonly ContentPartDefinition _partDefinition;

            public PartConfigurerImpl(ContentTypePartDefinition part)
                : base(part)
            {
                Current = part;
                _partDefinition = part.PartDefinition;
            }

            public override ContentTypePartDefinition Build()
            {
                return new ContentTypePartDefinition(Current.Name, _partDefinition, _settings)
                {
                    ContentTypeDefinition = Current.ContentTypeDefinition,
                };
            }
        }
    }
}