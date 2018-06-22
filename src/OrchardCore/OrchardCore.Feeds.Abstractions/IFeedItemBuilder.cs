using System.Threading.Tasks;
using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds
{
    public interface IFeedItemBuilder
    {
        Task PopulateAsync(FeedContext context);
    }
}
