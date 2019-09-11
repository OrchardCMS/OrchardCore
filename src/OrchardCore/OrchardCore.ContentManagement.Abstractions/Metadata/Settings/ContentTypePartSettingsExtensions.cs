using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypePartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder WithDisplayName(this ContentTypePartDefinitionBuilder builder, string displayName)
        {
            return builder.MergeSettings(new ContentTypePartSettings { DisplayName = displayName });
        }

        public static ContentTypePartDefinitionBuilder WithDescription(this ContentTypePartDefinitionBuilder builder, string description)
        {
            return builder.MergeSettings(new ContentTypePartSettings { Description = description });
        }

        public static ContentTypePartDefinitionBuilder WithPosition(this ContentTypePartDefinitionBuilder builder, string position)
        {
            return builder.MergeSettings(new ContentTypePartSettings { Position = position });
        }

        public static ContentTypePartDefinitionBuilder WithEditor(this ContentTypePartDefinitionBuilder builder, string editor)
        {
            return builder.MergeSettings(new ContentTypePartSettings { Editor = editor });
        }
    }
}
