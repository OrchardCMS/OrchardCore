using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.Contents.Services;
using Orchard.Lists.Indexes;
using YesSql;

namespace Orchard.Lists.Services
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
