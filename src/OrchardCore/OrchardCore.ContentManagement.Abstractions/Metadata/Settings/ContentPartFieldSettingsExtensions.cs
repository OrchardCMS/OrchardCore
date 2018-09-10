using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentPartFieldSettingsExtensions
    {
        public static ContentPartFieldDefinitionBuilder WithDisplayName(this ContentPartFieldDefinitionBuilder builder, string displayName)
        {
            return builder.WithSetting(nameof(ContentPartFieldSettings.DisplayName), displayName);
        }

        public static ContentPartFieldDefinitionBuilder WithDescription(this ContentPartFieldDefinitionBuilder builder, string description)
        {
            return builder.WithSetting(nameof(ContentPartFieldSettings.Description), description);
        }

        public static ContentPartFieldDefinitionBuilder WithEditor(this ContentPartFieldDefinitionBuilder builder, string editor)
        {
            return builder.WithSetting(nameof(ContentPartFieldSettings.Editor), editor);
        }

    }
}
