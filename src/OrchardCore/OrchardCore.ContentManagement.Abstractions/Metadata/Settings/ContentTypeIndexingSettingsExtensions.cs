using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypeIndexingSettingsExtensions
    {
        // TODO: WithSetting should append the "ContentTypeIndexingSettings" object itself
         public static ContentTypeDefinitionBuilder IsFullText(this ContentTypeDefinitionBuilder builder, bool isFullText = false)
        {
            return builder.WithSetting("IsFullText", isFullText.ToString());
        }

        public static ContentTypeDefinitionBuilder FullText(this ContentTypeDefinitionBuilder builder, string fulltext)
        {
            return builder.WithSetting("FullText", fulltext);
        }
    }
}
