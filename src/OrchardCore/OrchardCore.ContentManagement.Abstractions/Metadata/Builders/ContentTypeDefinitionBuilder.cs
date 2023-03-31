using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentManagement.Metadata.Builders
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
            if (!_name[0].IsLetter())
            {
                throw new ArgumentException("Content type name must start with a letter", "name");
            }
            if (!String.Equals(_name, _name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Content type name contains invalid characters", "name");
            }
            if (_name.IsReservedContentName())
            {
                throw new ArgumentException("Content type name is reserved for internal use", "name");
            }

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

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentTypeDefinitionBuilder WithSetting(string name, object value)
        {
            _settings[name] = JToken.FromObject(value);
            return this;
        }

        public ContentTypeDefinitionBuilder MergeSettings(JObject settings)
        {
            _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);
            return this;
        }

        public ContentTypeDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
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

        public ContentTypeDefinitionBuilder WithSettings<T>(T settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var jObject = JObject.FromObject(settings, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
            _settings[typeof(T).Name] = jObject;

            return this;
        }

        public ContentTypeDefinitionBuilder RemovePart(string partName)
        {
            var existingPart = _parts.SingleOrDefault(x => String.Equals(x.Name, partName, StringComparison.OrdinalIgnoreCase));
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
            var existingPart = _parts.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
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

        public Task<ContentTypeDefinitionBuilder> WithPartAsync(string name, string partName, Func<ContentTypePartDefinitionBuilder, Task> configurationAsync)
        {
            return WithPartAsync(name, new ContentPartDefinition(partName), configurationAsync);
        }

        public Task<ContentTypeDefinitionBuilder> WithPartAsync(string partName, Func<ContentTypePartDefinitionBuilder, Task> configurationAsync)
        {
            return WithPartAsync(partName, new ContentPartDefinition(partName), configurationAsync);
        }

        public async Task<ContentTypeDefinitionBuilder> WithPartAsync(string name, ContentPartDefinition partDefinition, Func<ContentTypePartDefinitionBuilder, Task> configurationAsync)
        {
            var existingPart = _parts.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

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

            await configurationAsync(configurer);

            _parts.Add(configurer.Build());

            return this;
        }

        private class PartConfigurerImpl : ContentTypePartDefinitionBuilder
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
                if (!Current.Name[0].IsLetter())
                {
                    throw new ArgumentException("Content part name must start with a letter", "name");
                }
                if (!String.Equals(Current.Name, Current.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Content part name contains invalid characters", "name");
                }

                return new ContentTypePartDefinition(Current.Name, _partDefinition, _settings)
                {
                    ContentTypeDefinition = Current.ContentTypeDefinition,
                };
            }
        }
    }
}
