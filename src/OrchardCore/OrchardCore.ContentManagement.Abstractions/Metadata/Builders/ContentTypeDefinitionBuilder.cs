using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentManagement.Metadata.Builders;

public class ContentTypeDefinitionBuilder
{
    private string _name;
    private string _displayName;
    private readonly List<ContentTypePartDefinition> _parts;
    private readonly JsonObject _settings;

    public ContentTypeDefinition Current { get; }

    public ContentTypeDefinitionBuilder()
        : this(new ContentTypeDefinition(null, null))
    {
    }

    public ContentTypeDefinitionBuilder(ContentTypeDefinition existing)
    {
        Current = existing;

        if (existing == null)
        {
            _parts = [];
            _settings = [];
        }
        else
        {
            _name = existing.Name;
            _displayName = existing.DisplayName;
            _parts = existing.Parts.ToList();
            _settings = existing.Settings.Clone();
        }
    }

    public ContentTypeDefinition Build()
    {
        if (!char.IsLetter(_name[0]))
        {
            throw new ArgumentException("Content type name must start with a letter", "name");
        }
        if (!string.Equals(_name, _name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
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

    public ContentTypeDefinitionBuilder MergeSettings(JsonObject settings)
    {
        _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);
        return this;
    }

    public ContentTypeDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
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

    public ContentTypeDefinitionBuilder WithSettings<T>(T settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var jObject = JObject.FromObject(settings, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
        _settings[typeof(T).Name] = jObject;

        return this;
    }

    public ContentTypeDefinitionBuilder RemovePart<TPart>()
        where TPart : ContentPart
        => RemovePart(typeof(TPart).Name);

    public ContentTypeDefinitionBuilder RemovePart(string partName)
    {
        var existingPart = _parts.SingleOrDefault(x => string.Equals(x.Name, partName, StringComparison.OrdinalIgnoreCase));
        if (existingPart != null)
        {
            _parts.Remove(existingPart);
        }
        return this;
    }

    public ContentTypeDefinitionBuilder WithPart(string partName)
        => WithPart(partName, configuration => { });

    public ContentTypeDefinitionBuilder WithPart(string name, string partName)
        => WithPart(name, new ContentPartDefinition(partName), configuration => { });

    public ContentTypeDefinitionBuilder WithPart(string partName, Action<ContentTypePartDefinitionBuilder> configuration)
        => WithPart(partName, new ContentPartDefinition(partName), configuration);

    public ContentTypeDefinitionBuilder WithPart(string name, ContentPartDefinition partDefinition, Action<ContentTypePartDefinitionBuilder> configuration)
    {
        var existingPart = _parts.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existingPart != null)
        {
            _parts.Remove(existingPart);
        }
        else
        {
            existingPart = new ContentTypePartDefinition(name, partDefinition, [])
            {
                ContentTypeDefinition = Current,
            };
        }

        var configurer = new PartConfigurerImpl(existingPart);
        configuration(configurer);
        _parts.Add(configurer.Build());
        return this;
    }

    public ContentTypeDefinitionBuilder WithPart(string name, string partName, Action<ContentTypePartDefinitionBuilder> configuration)
        => WithPart(name, new ContentPartDefinition(partName), configuration);

    public ContentTypeDefinitionBuilder WithPart<TPart>() where TPart : ContentPart
        => WithPart(typeof(TPart).Name, configuration => { });

    public ContentTypeDefinitionBuilder WithPart<TPart>(string name) where TPart : ContentPart
        => WithPart(name, new ContentPartDefinition(typeof(TPart).Name), configuration => { });

    public ContentTypeDefinitionBuilder WithPart<TPart>(string name, Action<ContentTypePartDefinitionBuilder> configuration) where TPart : ContentPart
        => WithPart(name, new ContentPartDefinition(typeof(TPart).Name), configuration);

    public ContentTypeDefinitionBuilder WithPart<TPart>(Action<ContentTypePartDefinitionBuilder> configuration) where TPart : ContentPart
        => WithPart(typeof(TPart).Name, configuration);

    public Task<ContentTypeDefinitionBuilder> WithPartAsync(string name, string partName, Func<ContentTypePartDefinitionBuilder, Task> configurationAsync)
        => WithPartAsync(name, new ContentPartDefinition(partName), configurationAsync);

    public Task<ContentTypeDefinitionBuilder> WithPartAsync(string partName, Func<ContentTypePartDefinitionBuilder, Task> configurationAsync)
        => WithPartAsync(partName, new ContentPartDefinition(partName), configurationAsync);

    public async Task<ContentTypeDefinitionBuilder> WithPartAsync(string name, ContentPartDefinition partDefinition, Func<ContentTypePartDefinitionBuilder, Task> configurationAsync)
    {
        var existingPart = _parts.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (existingPart != null)
        {
            _parts.Remove(existingPart);
        }
        else
        {
            existingPart = new ContentTypePartDefinition(name, partDefinition, [])
            {
                ContentTypeDefinition = Current,
            };
        }

        var configurer = new PartConfigurerImpl(existingPart);

        await configurationAsync(configurer);

        _parts.Add(configurer.Build());

        return this;
    }

    private sealed class PartConfigurerImpl : ContentTypePartDefinitionBuilder
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
            if (!char.IsLetter(Current.Name[0]))
            {
                throw new ArgumentException("Content part name must start with a letter", "name");
            }

            if (!string.Equals(Current.Name, Current.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
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
