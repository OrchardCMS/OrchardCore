namespace OrchardCore.Feeds.Models
{
    /// <summary>
    /// Used to gather custom Feed properties for a content item.
    /// </summary>
    public class FeedMetadata
    {
        public bool EnableFeedProxyUrl { get; set; } = true;

        public string FeedProxyUrl { get; set; }
    }
}
