using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentPartFieldSettingsExtensions
    {
        public static ContentPartFieldDefinitionBuilder WithDisplayName(this ContentPartFieldDefinitionBuilder builder, string displayName)
        {
            return builder.MergeSettings(new ContentPartFieldSettings { DisplayName = displayName });
        }

        public static ContentPartFieldDefinitionBuilder WithDescription(this ContentPartFieldDefinitionBuilder builder, string description)
        {
            return builder.MergeSettings(new ContentPartFieldSettings { Description = description });
        }

        public static ContentPartFieldDefinitionBuilder WithEditor(this ContentPartFieldDefinitionBuilder builder, string editor)
        {
            return builder.MergeSettings(new ContentPartFieldSettings { Editor = editor });
        }

        public static ContentPartFieldDefinitionBuilder WithDisplayMode(this ContentPartFieldDefinitionBuilder builder, string displayMode)
        {
            return builder.MergeSettings(new ContentPartFieldSettings { DisplayMode = displayMode });
        }

    }
}
