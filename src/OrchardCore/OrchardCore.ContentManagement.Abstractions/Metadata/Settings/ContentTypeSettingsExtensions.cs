using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypeSettingsExtensions
    {
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true)
        {
            return builder.MergeSettings(new ContentTypeSettings { Creatable = creatable });
        }

        public static ContentTypeDefinitionBuilder Listable(this ContentTypeDefinitionBuilder builder, bool listable = true)
        {
            return builder.MergeSettings(new ContentTypeSettings { Listable = listable });
        }

        public static ContentTypeDefinitionBuilder Draftable(this ContentTypeDefinitionBuilder builder, bool draftable = true)
        {
            return builder.MergeSettings(new ContentTypeSettings { Draftable = draftable });
        }

        public static ContentTypeDefinitionBuilder Versionable(this ContentTypeDefinitionBuilder builder, bool versionable = true)
        {
            return builder.MergeSettings(new ContentTypeSettings { Versionable = versionable });
        }

        public static ContentTypeDefinitionBuilder Securable(this ContentTypeDefinitionBuilder builder, bool securable = true)
        {
            return builder.MergeSettings(new ContentTypeSettings { Securable = securable });
        }

        public static ContentTypeDefinitionBuilder Stereotype(this ContentTypeDefinitionBuilder builder, string stereotype)
        {
            return builder.MergeSettings(new ContentTypeSettings { Stereotype = stereotype });
        }
    }
}
