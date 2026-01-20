using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypeSettingsExtensions
    {
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Creatable = creatable);
        }

        public static ContentTypeDefinitionBuilder Listable(this ContentTypeDefinitionBuilder builder, bool listable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Listable = listable);
        }

        public static ContentTypeDefinitionBuilder Draftable(this ContentTypeDefinitionBuilder builder, bool draftable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Draftable = draftable);
        }

        public static ContentTypeDefinitionBuilder Versionable(this ContentTypeDefinitionBuilder builder, bool versionable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Versionable = versionable);
        }

        public static ContentTypeDefinitionBuilder Securable(this ContentTypeDefinitionBuilder builder, bool securable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Securable = securable);
        }

        public static ContentTypeDefinitionBuilder Stereotype(this ContentTypeDefinitionBuilder builder, string stereotype)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Stereotype = stereotype);
        }

        public static ContentTypeDefinitionBuilder WithDescription(this ContentTypeDefinitionBuilder builder, string description)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Description = description);
        }
    }
}
