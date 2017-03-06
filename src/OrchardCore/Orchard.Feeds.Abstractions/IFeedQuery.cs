using System.Threading.Tasks;
using Orchard.Feeds.Models;

namespace Orchard.Feeds
{
    public interface IFeedQuery
    {
        Task ExecuteAsync(FeedContext context);
    }
}