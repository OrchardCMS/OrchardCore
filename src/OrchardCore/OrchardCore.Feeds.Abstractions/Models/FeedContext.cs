using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Feeds.Models
{
    public class FeedContext
    {
        public FeedContext(IUpdateModel updater, string format)
        {
            Updater = updater;
            Format = format;
            Response = new FeedResponse();
        }

        public IUpdateModel Updater { get; set; }
        public string Format { get; set; }
        public FeedResponse Response { get; set; }
        public IFeedBuilder Builder { get; set; }
    }
}
