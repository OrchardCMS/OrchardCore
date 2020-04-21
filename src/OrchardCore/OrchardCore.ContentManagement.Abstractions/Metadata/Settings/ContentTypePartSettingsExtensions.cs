using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypePartSettingsExtensions
    {
        public static ContentTypePartDefinitionBuilder WithDisplayName(this ContentTypePartDefinitionBuilder builder, string displayName)
        {
            return builder.MergeSettings<ContentTypePartSettings>(x => x.DisplayName = displayName);
        }

        public static ContentTypePartDefinitionBuilder WithDescription(this ContentTypePartDefinitionBuilder builder, string description)
        {
            return builder.MergeSettings<ContentTypePartSettings>(x => x.Description = description);
        }

        public static ContentTypePartDefinitionBuilder WithPosition(this ContentTypePartDefinitionBuilder builder, string position)
        {
            return builder.MergeSettings<ContentTypePartSettings>(x => x.Position = position);
        }

        public static ContentTypePartDefinitionBuilder WithDisplayMode(this ContentTypePartDefinitionBuilder builder, string displayMode)
        {
            return builder.MergeSettings<ContentTypePartSettings>(x => x.DisplayMode = displayMode);
        }

        public static ContentTypePartDefinitionBuilder WithEditor(this ContentTypePartDefinitionBuilder builder, string editor)
        {
            return builder.MergeSettings<ContentTypePartSettings>(x => x.Editor = editor);
        }
    }
}
