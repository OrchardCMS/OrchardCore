using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Handlers
{
    public class WorkflowInstanceRoutesHandler : WorkflowHandlerBase
    {
        private readonly IWorkflowInstanceRouteEntries _workflowRouteEntries;
        private readonly IWorkflowTypeStore _workflowDefinitionStore;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowInstanceRoutesHandler(
            IWorkflowInstanceRouteEntries workflowRouteEntries,
            IWorkflowTypeStore workflowDefinitionStore,
            IActivityLibrary activityLibrary
        )
        {
            _workflowRouteEntries = workflowRouteEntries;
            _workflowDefinitionStore = workflowDefinitionStore;
            _activityLibrary = activityLibrary;
        }

        public async override Task CreatedAsync(WorkflowCreatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
        }

        public async override Task UpdatedAsync(WorkflowUpdatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
        }

        public override Task DeletedAsync(WorkflowDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.Workflow.WorkflowId);
            return Task.CompletedTask;
        }

        private async Task UpdateRouteEntriesAsync(WorkflowContext context)
        {
            var workflowInstanceRecord = context.Workflow;
            var workflowDefinitionRecord = await _workflowDefinitionStore.GetAsync(workflowInstanceRecord.WorkflowTypeId);
            var entries = WorkflowInstanceRouteEntries.GetWorkflowInstanceRoutesEntries(workflowDefinitionRecord, context.Workflow, _activityLibrary);

            _workflowRouteEntries.AddEntries(entries);
        }
    }
}
