using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentManagement.Metadata.Builders;

public class ContentPartDefinitionBuilder
{
    private readonly ContentPartDefinition _part;
    private readonly List<ContentPartFieldDefinition> _fields;
    private readonly JsonObject _settings;

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
            _fields = [];
            _settings = [];
        }
        else
        {
            Name = existing.Name;
            _fields = existing.Fields.ToList();
            _settings = existing.Settings.Clone();
        }
    }

    public string Name { get; private set; }

    public ContentPartDefinition Build()
    {
        if (!char.IsLetter(Name[0]))
        {
            throw new ArgumentException("Content part name must start with a letter", "name");
        }
        if (!string.Equals(Name, Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
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
        var existingField = _fields.SingleOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
        if (existingField != null)
        {
            _fields.Remove(existingField);
        }

        return this;
    }

    public ContentPartDefinitionBuilder MergeSettings(JsonObject settings)
    {
        _settings.Merge(settings, ContentBuilderSettings.JsonMergeSettings);

        return this;
    }

    public ContentPartDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
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

    public ContentPartDefinitionBuilder WithSettings<T>(T settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        var jObject = JObject.FromObject(settings, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
        _settings[typeof(T).Name] = jObject;

        return this;
    }

    public ContentPartDefinitionBuilder WithField(string fieldName)
        => WithField(fieldName, configuration => { });

    public ContentPartDefinitionBuilder WithField(string fieldName, Action<ContentPartFieldDefinitionBuilder> configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));

        var existingField = _fields.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
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
            existingField = new ContentPartFieldDefinition(null, fieldName, []);
        }

        var configurer = new FieldConfigurerImpl(existingField, _part);

        configuration(configurer);

        var fieldDefinition = configurer.Build();

        var settings = fieldDefinition.GetSettings<ContentPartFieldSettings>();

        if (string.IsNullOrEmpty(settings.DisplayName))
        {
            // If there is no display name, let's use the field name by default.
            settings.DisplayName = fieldName;
            fieldDefinition.Settings.Remove(nameof(ContentPartFieldSettings));
            fieldDefinition.Settings.Add(nameof(ContentPartFieldSettings), JNode.FromObject(settings));
        }

        _fields.Add(fieldDefinition);

        return this;
    }

    public ContentPartDefinitionBuilder WithField<TField>(string fieldName)
        => WithField(fieldName, configuration => configuration.OfType(typeof(TField).Name));

    public ContentPartDefinitionBuilder WithField<TField>(string fieldName, Action<ContentPartFieldDefinitionBuilder> configuration)
        => WithField(fieldName, field =>
        {
            configuration(field);

            field.OfType(typeof(TField).Name);
        });

    public Task<ContentPartDefinitionBuilder> WithFieldAsync<TField>(string fieldName, Func<ContentPartFieldDefinitionBuilder, Task> configuration)
        => WithFieldAsync(fieldName, async field =>
        {
            await configuration(field);

            field.OfType(typeof(TField).Name);
        });

    public async Task<ContentPartDefinitionBuilder> WithFieldAsync(string fieldName, Func<ContentPartFieldDefinitionBuilder, Task> configurationAsync)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));

        var existingField = _fields.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));

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
            existingField = new ContentPartFieldDefinition(null, fieldName, []);
        }

        var configurer = new FieldConfigurerImpl(existingField, _part);

        await configurationAsync(configurer);

        var fieldDefinition = configurer.Build();

        var settings = fieldDefinition.GetSettings<ContentPartFieldSettings>();

        if (string.IsNullOrEmpty(settings.DisplayName))
        {
            // If there is no display name, let's use the field name by default.
            settings.DisplayName = fieldName;
            fieldDefinition.Settings.Remove(nameof(ContentPartFieldSettings));
            fieldDefinition.Settings.Add(nameof(ContentPartFieldSettings), JNode.FromObject(settings));
        }

        _fields.Add(fieldDefinition);

        return this;
    }

    private sealed class FieldConfigurerImpl : ContentPartFieldDefinitionBuilder
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
            if (!char.IsLetter(_fieldName[0]))
            {
                throw new ArgumentException("Content field name must start with a letter", "name");
            }
            if (!string.Equals(_fieldName, _fieldName.ToSafeName(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Content field name contains invalid characters", "name");
            }

            return new ContentPartFieldDefinition(_fieldDefinition, _fieldName, _settings);
        }

        public override string Name
            => _fieldName;

        public override string FieldType
            => _fieldDefinition.Name;


        public override string PartName
            => _partDefinition.Name;

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
