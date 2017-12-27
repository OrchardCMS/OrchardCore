using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
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

        public override Task PublishedAsync(PublishContentContext context)
        {
            return _workflowManager.TriggerEventAsync(nameof(ContentPublishedEvent), () => new RouteValueDictionary(new { Content = context.ContentItem }));
        }

        public override void Removed(RemoveContentContext context)
        {
        }

        public override void Unpublished(PublishContentContext context)
        {

        }
    }
}