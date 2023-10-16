using System;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata.Builders
{
    public abstract class ContentPartFieldDefinitionBuilder : BuilderBase
    {
        public ContentPartFieldDefinition Current { get; private set; }
        public abstract string Name { get; }
        public abstract string FieldType { get; }
        public abstract string PartName { get; }

        protected ContentPartFieldDefinitionBuilder(ContentPartFieldDefinition field) : base(field.Settings)
        {
            Current = field;
        }

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentPartFieldDefinitionBuilder WithSetting(string name, string value)
        {
            WithSettingImpl(name, value);
            return this;
        }

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentPartFieldDefinitionBuilder WithSetting(string name, string[] values)
        {
            WithSettingImpl(name, values);
            return this;
        }

        public ContentPartFieldDefinitionBuilder MergeSettings(JsonObject settings)
        {
            MergeSettingsImpl(settings);
            return this;
        }

        [Obsolete("Use MergeSettings<T>. This will be removed in a future version.")]
        public ContentPartFieldDefinitionBuilder MergeSettings(object model)
        {
            MergeSettingsImpl(model);
            return this;
        }

        public ContentPartFieldDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
        {
            MergeSettingsImpl(setting);
            return this;
        }

        public ContentPartFieldDefinitionBuilder WithSettings<T>(T settings)
        {
            WithSettingsImpl(settings);
            return this;
        }

        public abstract ContentPartFieldDefinitionBuilder OfType(ContentFieldDefinition fieldDefinition);
        public abstract ContentPartFieldDefinitionBuilder OfType(string fieldType);
        public abstract ContentPartFieldDefinition Build();
    }
}
