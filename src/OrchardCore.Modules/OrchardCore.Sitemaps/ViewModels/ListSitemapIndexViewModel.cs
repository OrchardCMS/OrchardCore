namespace OrchardCore.Sitemaps.ViewModels;

public class ListSitemapIndexViewModel
{
    public IList<SitemapIndexListEntry> SitemapIndexes { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();
    public dynamic Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the rows, the toolbar and the pager in the configured layout.
    /// </summary>
    public dynamic List { get; set; }
}

public class SitemapIndexListEntry
{
    public string SitemapId { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
}
