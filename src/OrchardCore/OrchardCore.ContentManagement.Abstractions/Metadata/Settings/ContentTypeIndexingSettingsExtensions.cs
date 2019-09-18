using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public static class ContentTypeIndexingSettingsExtensions
    {
        public static ContentTypeDefinitionBuilder IsFullText(this ContentTypeDefinitionBuilder builder, bool isFullText = false)
        {
            return builder.MergeSettings<ContentTypeIndexingSettings>(x => x.IsFullText = isFullText);
        }

        public static ContentTypeDefinitionBuilder FullText(this ContentTypeDefinitionBuilder builder, string fulltext)
        {
            return builder.MergeSettings<ContentTypeIndexingSettings>(x => x.FullText = fulltext);
        }
    }
}
