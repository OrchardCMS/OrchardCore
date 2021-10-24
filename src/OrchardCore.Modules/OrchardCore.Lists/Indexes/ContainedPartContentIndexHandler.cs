using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Indexes
{
    public class ContainedPartContentIndexHandler : IContentItemIndexHandler
    {
        public const string ListContentItemIdKey = "Content.ContentItem.ContainedPart.ListContentItemId";
        public const string OrderKey = "Content.ContentItem.ContainedPart.Order";

        public Task BuildIndexAsync(BuildIndexContext context)
        {
            var parent = context.ContentItem.As<ContainedPart>();

            if (parent == null)
            {
                return Task.CompletedTask;
            }

            context.DocumentIndex.Set(
                ListContentItemIdKey,
                parent.ListContentItemId,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                OrderKey,
                parent.Order,
                DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}
