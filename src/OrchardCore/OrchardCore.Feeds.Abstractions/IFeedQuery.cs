using System.Threading.Tasks;
using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds
{
    public interface IFeedQuery
    {
        Task ExecuteAsync(FeedContext context);
    }
}
