using Orchard.ContentManagement;

namespace Orchard.Lists.Feeds
{
    public class ListFeedEditViewModel
    {
        public string FeedProxyUrl { get; set; }
        public int FeedItemsCount { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}
