namespace OrchardCore.Contents.SitemapNodes
{
    public class ContentTypesSitemapNodeViewModel
    {
        public bool ShowAll { get; set; }
        public string IconClass { get; set; }
        public ContentTypeSitemapEntryViewModel[] ContentTypes { get; set; } = new ContentTypeSitemapEntryViewModel[] { };
    }

    public class ContentTypeSitemapEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string ContentTypeId { get; set; }
        public string IconClass { get; set; }
    }
}
