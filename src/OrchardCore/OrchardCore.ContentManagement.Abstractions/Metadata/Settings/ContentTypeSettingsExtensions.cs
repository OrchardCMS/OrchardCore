using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypeSettingsExtensions
    {
        // TODO: WithSetting should append the "ContentTypeSettings" object itself
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true)
        {
            return builder.WithSetting("Creatable", creatable.ToString());
        }

        public static ContentTypeDefinitionBuilder Listable(this ContentTypeDefinitionBuilder builder, bool listable = true)
        {
            return builder.WithSetting("Listable", listable.ToString());
        }

        public static ContentTypeDefinitionBuilder Draftable(this ContentTypeDefinitionBuilder builder, bool draftable = true)
        {
            return builder.WithSetting("Draftable", draftable.ToString());
        }

        public static ContentTypeDefinitionBuilder Versionable(this ContentTypeDefinitionBuilder builder, bool versionable = true)
        {
            return builder.WithSetting("Versionable", versionable.ToString());
        }

        public static ContentTypeDefinitionBuilder Securable(this ContentTypeDefinitionBuilder builder, bool securable = true)
        {
            return builder.WithSetting("Securable", securable.ToString());
        }

        public static ContentTypeDefinitionBuilder Stereotype(this ContentTypeDefinitionBuilder builder, string stereotype)
        {
            return builder.WithSetting("Stereotype", stereotype);
        }
    }
}
