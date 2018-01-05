using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Indexing
{
    public class CreateIndexingTaskContentHandler : ContentHandlerBase
    {
        private readonly IIndexingTaskManager _indexingTaskManager;

        public CreateIndexingTaskContentHandler(IIndexingTaskManager indexingTaskManager)
        {
            _indexingTaskManager = indexingTaskManager;
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Update);
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Delete);
        }
    }
}
