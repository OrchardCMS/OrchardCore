using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Builders;

public class ContentItemsQueryContext
{
    /// <summary>
    /// Content items to include in sitemap.
    /// </summary>
    public IEnumerable<ContentItem> ContentItems { get; set; } = [];

    /// <summary>
    /// Reference content items that may be used to perform a lookup for url alternatives.
    /// </summary>
    public IEnumerable<ContentItem> ReferenceContentItems { get; set; } = [];
}
