using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Handlers
{
    internal class WorkflowRoutesHandler : WorkflowHandlerBase
    {
        private readonly IWorkflowInstanceRouteEntries _workflowRouteEntries;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowRoutesHandler(
            IWorkflowInstanceRouteEntries workflowRouteEntries,
            IWorkflowTypeStore workflowTypeStore,
            IActivityLibrary activityLibrary
        )
        {
            _workflowRouteEntries = workflowRouteEntries;
            _workflowTypeStore = workflowTypeStore;
            _activityLibrary = activityLibrary;
        }

        public override Task CreatedAsync(WorkflowCreatedContext context)
        {
            return UpdateRouteEntriesAsync(context);
        }

        public override Task UpdatedAsync(WorkflowUpdatedContext context)
        {
            return UpdateRouteEntriesAsync(context);
        }

        public override Task DeletedAsync(WorkflowDeletedContext context)
        {
            return _workflowRouteEntries.RemoveEntriesAsync(context.Workflow.WorkflowId);
        }

        private async Task UpdateRouteEntriesAsync(WorkflowContext context)
        {
            var workflow = context.Workflow;
            var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);
            var entries = WorkflowInstanceRouteEntries.GetWorkflowRoutesEntries(workflowType, context.Workflow, _activityLibrary);

            await _workflowRouteEntries.AddEntriesAsync(entries);
        }
    }
}
