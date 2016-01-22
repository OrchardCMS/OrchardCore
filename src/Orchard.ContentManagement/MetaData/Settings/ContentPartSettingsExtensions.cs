using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentManagement.Metadata.Settings
{
    public static class ContentPartSettingsExtensions
    {
        // TODO: WithSetting should append the "ContentTypeSettings" object itself
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true)
        {
            return builder.WithSetting("ContentTypeSettings.Creatable", creatable.ToString());
        }

        public static ContentTypeDefinitionBuilder Listable(this ContentTypeDefinitionBuilder builder, bool listable = true)
        {
            return builder.WithSetting("ContentTypeSettings.Listable", listable.ToString());
        }

        public static ContentTypeDefinitionBuilder Draftable(this ContentTypeDefinitionBuilder builder, bool draftable = true)
        {
            return builder.WithSetting("ContentTypeSettings.Draftable", draftable.ToString());
        }

        public static ContentTypeDefinitionBuilder Securable(this ContentTypeDefinitionBuilder builder, bool securable = true)
        {
            return builder.WithSetting("ContentTypeSettings.Securable", securable.ToString());
        }

        public static ContentPartDefinitionBuilder Attachable(this ContentPartDefinitionBuilder builder, bool attachable = true)
        {
            return builder.WithSetting("ContentPartSettings.Attachable", attachable.ToString());
        }

        public static ContentPartDefinitionBuilder WithDescription(this ContentPartDefinitionBuilder builder, string description)
        {
            return builder.WithSetting("ContentPartSettings.Description", description);
        }
    }
}
