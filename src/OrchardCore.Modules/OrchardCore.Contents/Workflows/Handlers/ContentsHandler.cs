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

        public override async Task CreatedAsync(CreateContentContext context)
        {
            await TriggerWorkflowEventAsync(nameof(ContentCreatedEvent), context.ContentItem);
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            await TriggerWorkflowEventAsync(nameof(ContentPublishedEvent), context.ContentItem);
        }

        public override async Task RemovedAsync(RemoveContentContext context)
        {
            await TriggerWorkflowEventAsync(nameof(ContentDeletedEvent), context.ContentItem);
        }

        private async Task TriggerWorkflowEventAsync(string name, ContentItem contentItem)
        {
            await _workflowManager.TriggerEventAsync(name,
                input: new { ContentItem = contentItem },
                correlationId: contentItem.ContentItemId
            );
        }
    }
}