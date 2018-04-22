using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Handlers
{
    public class WorkflowDefinitionRoutesHandler : WorkflowDefinitionHandlerBase
    {
        private readonly IWorkflowDefinitionRouteEntries _workflowRouteEntries;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowDefinitionRoutesHandler(IWorkflowDefinitionRouteEntries workflowRouteEntries, IActivityLibrary activityLibrary)
        {
            _workflowRouteEntries = workflowRouteEntries;
            _activityLibrary = activityLibrary;
        }

        public override Task CreatedAsync(WorkflowDefinitionCreatedContext context)
        {
            UpdateRouteEntries(context);
            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(WorkflowDefinitionUpdatedContext context)
        {
            UpdateRouteEntries(context);
            return Task.CompletedTask;
        }

        public override Task DeletedAsync(WorkflowDefinitionDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowDefinition.Id.ToString());
            return Task.CompletedTask;
        }

        private void UpdateRouteEntries(WorkflowDefinitionContext context)
        {
            var entries = WorkflowDefinitionRouteEntries.GetWorkflowDefinitionRoutesEntries(context.WorkflowDefinition, _activityLibrary);
            _workflowRouteEntries.AddEntries(entries);
        }
    }
}
