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

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            return _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Update);
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            return _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Update);
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Update);
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            if (context.NoActiveVersionLeft)
            {
                return _indexingTaskManager.CreateTaskAsync(context.ContentItem, IndexingTaskTypes.Delete);
            }

            return Task.CompletedTask;
        }
    }
}
