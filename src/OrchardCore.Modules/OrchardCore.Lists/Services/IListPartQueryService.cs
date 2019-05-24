using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Services
{
    public interface IListPartQueryService
    {
        Task<IEnumerable<ContentItem>> QueryListItemsAsync(string contentItemId, bool enableOrdering, PagerSlim pager, bool publishedOnly);
    }
}
