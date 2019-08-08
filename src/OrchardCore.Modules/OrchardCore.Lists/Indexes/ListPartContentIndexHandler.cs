using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Indexes
{
    public class ListPartContentIndexHandler : IContentItemIndexHandler
    {
        public const string ParentKey = "Content.ContentItem.Parent";

        public Task BuildIndexAsync(BuildIndexContext context)
        {
            var parent = context.ContentItem.As<ContainedPart>();

            if (parent == null)
            {
                return Task.CompletedTask;
            }

            context.DocumentIndex.Set(
                ParentKey,
                parent.ListContentItemId,
                DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}
