using System;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata.Builders
{
    public abstract class ContentTypePartDefinitionBuilder : BuilderBase
    {
        public ContentTypePartDefinition Current { get; protected set; }
        public string Name { get; private set; }
        public string PartName { get; private set; }
        public string TypeName { get; private set; }

        protected ContentTypePartDefinitionBuilder(ContentTypePartDefinition part) : base(part.Settings)
        {
            Current = part;
            Name = part.Name;
            PartName = part.PartDefinition.Name;
            TypeName = part.ContentTypeDefinition != null ? part.ContentTypeDefinition.Name : default;
        }

        public ContentTypePartDefinitionBuilder WithSettings<T>(T settings)
        {
            WithSettingsImpl(settings);
            return this;
        }

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentTypePartDefinitionBuilder WithSetting(string name, string value)
        {
            WithSettingImpl(name, value);
            return this;
        }

        [Obsolete("Use WithSettings<T>. This will be removed in a future version.")]
        public ContentTypePartDefinitionBuilder WithSetting(string name, string[] values)
        {
            WithSettingImpl(name, values);
            return this;
        }

        public ContentTypePartDefinitionBuilder MergeSettings(JsonObject settings)
        {
            MergeSettingsImpl(settings);
            return this;
        }

        public ContentTypePartDefinitionBuilder MergeSettings<T>(Action<T> setting) where T : class, new()
        {
            MergeSettingsImpl(setting);
            return this;
        }

        public abstract ContentTypePartDefinition Build();
    }
}
