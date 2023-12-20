namespace OrchardCore.Search.Azure.CognitiveSearch;
public static class IndexingConstants
{
    public const string OwnerKey = "Content_ContentItem_Owner";
    public const string AuthorKey = "Content_ContentItem_Author";
    public const string ContentTypeKey = "Content_ContentItem_ContentType";
    public const string ContentItemIdKey = "ContentItemId";
    public const string ContentItemVersionIdKey = "ContentItemVersionId";
    public const string CreatedUtcKey = "Content_ContentItem_CreatedUtc";
    public const string LatestKey = "Content_ContentItem_Latest";
    public const string ModifiedUtcKey = "Content_ContentItem_ModifiedUtc";
    public const string PublishedKey = "Content_ContentItem_Published";
    public const string PublishedUtcKey = "Content_ContentItem_PublishedUtc";
    public const string DisplayTextKey = "Content_ContentItem_DisplayText";
    public const string FullTextKeySuggester = "Content_ContentItem_FullText_Suggester";
    public const string FullTextKey = "Content_ContentItem_FullText";
    public const string ContainedPartKey = "Content_ContentItem_ContainedPart";
}
