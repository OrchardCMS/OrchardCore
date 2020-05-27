using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Handlers
{
    public class ContentsHandler : ContentHandlerBase
    {
        private readonly IWorkflowManager _workflowManager;

        public ContentsHandler(IWorkflowManager workflowManager)
        {
            _workflowManager = workflowManager;
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentCreatedEvent), context.ContentItem);
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentUpdatedEvent), context.ContentItem);
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentPublishedEvent), context.ContentItem);
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentUnpublishedEvent), context.ContentItem);
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentDeletedEvent), context.ContentItem);
        }

        public override Task VersionedAsync(VersionContentContext context)
        {
            return TriggerWorkflowEventAsync(nameof(ContentVersionedEvent), context.ContentItem);
        }

        private Task TriggerWorkflowEventAsync(string name, ContentItem contentItem)
        {
            var eventInfo = new ContentEventInfo()
            {
                Name = name,
                ContentType = contentItem.ContentType,
                ContentItemId = contentItem.ContentItemId
            };

            return _workflowManager.TriggerEventAsync(name,
                input: new { ContentItem = contentItem, ContentEvent = eventInfo },
                correlationId: contentItem.ContentItemId
            );
        }
    }
}
