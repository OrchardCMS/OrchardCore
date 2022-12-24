namespace OrchardCore.ContentManagement.Handlers;

public class ImportContentFieldContext : ContentFieldContextBase
{
    /// <summary>
    /// When importing an item may exist in the database.
    /// </summary>
    public ContentItem OriginalContentItem { get; set; }

    public ImportContentFieldContext(ContentItem contentItem, ContentItem originalContentItem = null)
        : base(contentItem)
    {
        OriginalContentItem = originalContentItem;
    }
}
