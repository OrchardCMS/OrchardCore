using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Indexes
{
    public class ContainedPartContentIndexHandler : IContentItemIndexHandler
    {
        public const string ParentKey = "Content.ContentItem.ContainedPart";

        public Task BuildIndexAsync(BuildIndexContext context)
        {
            var parent = context.ContentItem.As<ContainedPart>();

            if (parent == null)
            {
                return Task.CompletedTask;
            }

            context.DocumentIndex.Set(
                ParentKey + ".ListContentItemId",
                parent.ListContentItemId,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                ParentKey + ".Order",
                parent.Order,
                DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}
