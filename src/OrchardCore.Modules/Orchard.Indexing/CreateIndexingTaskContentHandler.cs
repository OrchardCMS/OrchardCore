using Orchard.ContentManagement.Handlers;

namespace Orchard.Indexing
{
    public class CreateIndexingTaskContentHandler : ContentHandlerBase
    {
        private readonly IIndexingTaskManager _indexingTaskManager;

        public CreateIndexingTaskContentHandler(IIndexingTaskManager indexingTaskManager)
        {
            _indexingTaskManager = indexingTaskManager;
        }

        public override void Published(PublishContentContext context)
        {
            _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Update).Wait();
        }

        public override void Removed(RemoveContentContext context)
        {
            _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Delete).Wait();
        }
    }
}
