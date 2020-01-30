using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Services
{
    public interface IContainerService
    {
        Task<IEnumerable<ContentItem>> QueryContainedItemsAsync(string contentItemId, bool enableOrdering, PagerSlim pager, bool publishedOnly);
        Task UpdateContentItemOrdersAsync(IEnumerable<ContentItem> contentItems, int orderOfFirstItem);
        Task<IEnumerable<ContentItem>> GetContainedItemsAsync(string contentItemId);
        Task<IEnumerable<ContentItem>> GetContainerItemsAsync(string contentType);
        Task<int> GetNextOrderNumberAsync(string contentItemId);
    }
}
