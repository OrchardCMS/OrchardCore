using System.Threading.Tasks;
using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds
{
    public interface IFeedQueryProvider
    {
        Task<FeedQueryMatch> MatchAsync(FeedContext context);
    }

    public class FeedQueryMatch
    {
        public int Priority { get; set; }
        public IFeedQuery FeedQuery { get; set; }
    }
}
