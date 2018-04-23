using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Handlers
{
    public class WorkflowDefinitionRoutesHandler : WorkflowTypeHandlerBase
    {
        private readonly IWorkflowDefinitionRouteEntries _workflowRouteEntries;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowDefinitionRoutesHandler(IWorkflowDefinitionRouteEntries workflowRouteEntries, IActivityLibrary activityLibrary)
        {
            _workflowRouteEntries = workflowRouteEntries;
            _activityLibrary = activityLibrary;
        }

        public override Task CreatedAsync(WorkflowTypeCreatedContext context)
        {
            UpdateRouteEntries(context);
            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(WorkflowTypeUpdatedContext context)
        {
            UpdateRouteEntries(context);
            return Task.CompletedTask;
        }

        public override Task DeletedAsync(WorkflowTypeDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowType.Id.ToString());
            return Task.CompletedTask;
        }

        private void UpdateRouteEntries(WorkflowTypeContext context)
        {
            var entries = WorkflowDefinitionRouteEntries.GetWorkflowDefinitionRoutesEntries(context.WorkflowType, _activityLibrary);
            _workflowRouteEntries.AddEntries(entries);
        }
    }
}
