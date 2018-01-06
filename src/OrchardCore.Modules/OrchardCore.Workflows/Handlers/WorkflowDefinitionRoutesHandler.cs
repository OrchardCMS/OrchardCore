using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Handlers
{
    public class WorkflowDefinitionRoutesHandler : WorkflowDefinitionHandlerBase
    {
        private readonly IWorkflowDefinitionRouteEntries _workflowRouteEntries;
        private readonly IWorkflowDefinitionPathEntries _workflowPathEntries;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowDefinitionRoutesHandler(IWorkflowDefinitionRouteEntries workflowRouteEntries, IWorkflowDefinitionPathEntries workflowPathEntries, IActivityLibrary activityLibrary)
        {
            _workflowRouteEntries = workflowRouteEntries;
            _workflowPathEntries = workflowPathEntries;
            _activityLibrary = activityLibrary;
        }

        public override Task CreatedAsync(WorkflowDefinitionCreatedContext context)
        {
            UpdateRouteEntries(context);
            UpdatePathEntries(context);
            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(WorkflowDefinitionUpdatedContext context)
        {
            UpdateRouteEntries(context);
            UpdatePathEntries(context);
            return Task.CompletedTask;
        }

        public override Task DeletedAsync(WorkflowDefinitionDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowDefinition.Id.ToString());
            _workflowPathEntries.RemoveEntries(context.WorkflowDefinition.Id.ToString());
            return Task.CompletedTask;
        }

        private void UpdateRouteEntries(WorkflowDefinitionContext context)
        {
            var entries = WorkflowDefinitionRouteEntries.GetWorkflowDefinitionRoutesEntries(context.WorkflowDefinition, _activityLibrary);
            _workflowRouteEntries.AddEntries(entries);
        }

        private void UpdatePathEntries(WorkflowDefinitionContext context)
        {
            var entries = WorkflowDefinitionPathEntries.GetWorkflowPathEntries(context.WorkflowDefinition, _activityLibrary);
            _workflowPathEntries.AddEntries(entries);
        }
    }
}
