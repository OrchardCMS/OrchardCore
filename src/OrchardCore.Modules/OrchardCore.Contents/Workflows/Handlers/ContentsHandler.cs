using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Handlers
{
    public class ContentsHandler : ContentHandlerBase
    {
        public const string ContentItemInputKey = "ContentItem";

        private readonly IWorkflowManager _workflowManager;

        public ContentsHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentCreatedEvent), context.ContentItem);
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentPublishedEvent), context.ContentItem);
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentDeletedEvent), context.ContentItem);
        }

        private Task TriggerWorkflowEventAsync(string name, ContentItem contentItem)
        {
            return _workflowManager.TriggerEventAsync(name,
                input: new { ContentItem = contentItem },
                correlationId: contentItem.ContentItemId
            );
        }
    }
}
