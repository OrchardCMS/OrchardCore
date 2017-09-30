using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Lists.Indexes;
using YesSql;

namespace OrchardCore.Lists.Services
{
    public class ListPartContentApiFilter : IContentApiFilter
    {
        public Task FilterAsync(IQuery<ContentItem> query, string parentContentItemId)
        {
            if (parentContentItemId != null)
            {
                query.With<ContainedPartIndex>(x => x.ListContentItemId == parentContentItemId);
            }
            return Task.CompletedTask;
        }
    }
}
