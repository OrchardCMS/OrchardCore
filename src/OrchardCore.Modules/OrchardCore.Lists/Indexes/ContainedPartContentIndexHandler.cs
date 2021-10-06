using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Indexes
{
    public class ContainedPartContentIndexHandler : IContentItemIndexHandler
    {
        public const string key = "Content.ContentItem.ContainedPart";

        public Task BuildIndexAsync(BuildIndexContext context)
        {
            var parent = context.ContentItem.As<ContainedPart>();

            if (parent == null)
            {
                return Task.CompletedTask;
            }

            context.DocumentIndex.Set(
                key + ".ListContentItemId",
                parent.ListContentItemId,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                key + ".Order",
                parent.Order,
                DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}
