using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypeSettingsExtensions
    {
        public static ContentTypeDefinitionBuilder Creatable(this ContentTypeDefinitionBuilder builder, bool creatable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Creatable = creatable);
        }

        public static bool IsCreatable(this ContentTypeDefinition type)
        {
            return type.GetSettings<ContentTypeSettings>().Creatable;
        }

        public static ContentTypeDefinitionBuilder Listable(this ContentTypeDefinitionBuilder builder, bool listable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Listable = listable);
        }

        public static bool IsListable(this ContentTypeDefinition type)
        {
            return type.GetSettings<ContentTypeSettings>().Listable;
        }

        public static ContentTypeDefinitionBuilder Draftable(this ContentTypeDefinitionBuilder builder, bool draftable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Draftable = draftable);
        }

        public static bool IsDraftable(this ContentTypeDefinition type)
        {
            return type.GetSettings<ContentTypeSettings>().Draftable;
        }

        public static ContentTypeDefinitionBuilder Versionable(this ContentTypeDefinitionBuilder builder, bool versionable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Versionable = versionable);
        }

        public static bool IsVersionable(this ContentTypeDefinition type)
        {
            return type.GetSettings<ContentTypeSettings>().Versionable;
        }

        public static ContentTypeDefinitionBuilder Securable(this ContentTypeDefinitionBuilder builder, bool securable = true)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Securable = securable);
        }
        public static bool IsSecurable(this ContentTypeDefinition type)
        {
            return type.GetSettings<ContentTypeSettings>().Securable;
        }

        public static ContentTypeDefinitionBuilder Stereotype(this ContentTypeDefinitionBuilder builder, string stereotype)
        {
            return builder.MergeSettings<ContentTypeSettings>(x => x.Stereotype = stereotype);
        }
    }
}
