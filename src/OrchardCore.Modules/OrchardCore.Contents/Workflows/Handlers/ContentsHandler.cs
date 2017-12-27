using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
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
            return _workflowManager.TriggerEventAsync(nameof(ContentCreatedEvent), new { Content = context.ContentItem });
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return _workflowManager.TriggerEventAsync(nameof(ContentPublishedEvent), new { Content = context.ContentItem });
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return _workflowManager.TriggerEventAsync(nameof(ContentDeletedEvent), new { Content = context.ContentItem });
        }
    }
}