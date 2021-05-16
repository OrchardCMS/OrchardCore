namespace OrchardCore.ContentManagement.Handlers
{
    public class ImportContentContext : ContentContextBase
    {
        /// <summary>
        /// When importing an item may exist in the database.
        /// </summary>
        public ContentItem OriginalContentItem { get; set; }

        public ImportContentContext(ContentItem contentItem, ContentItem originalContentItem = null) : base(contentItem)
        {
            OriginalContentItem = originalContentItem;
        }
    }
}
