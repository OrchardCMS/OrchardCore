using OrchardCore.ContentManagement;

namespace OrchardCore.Lists.Feeds
{
    public class ListFeedEditViewModel
    {
        public string FeedProxyUrl { get; set; }
        public int FeedItemsCount { get; set; } = ListFeedQuery.DefaultItemsCount;
        public ContentItem ContentItem { get; set; }
    }
}
