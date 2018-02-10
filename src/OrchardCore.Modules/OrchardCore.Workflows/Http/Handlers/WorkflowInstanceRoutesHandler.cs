using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Handlers
{
    public class WorkflowInstanceRoutesHandler : WorkflowInstanceHandlerBase
    {
        private readonly IWorkflowInstanceRouteEntries _workflowRouteEntries;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowInstanceRoutesHandler(
            IWorkflowInstanceRouteEntries workflowRouteEntries,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IActivityLibrary activityLibrary
        )
        {
            _workflowRouteEntries = workflowRouteEntries;
            _workflowDefinitionStore = workflowDefinitionStore;
            _activityLibrary = activityLibrary;
        }

        public async override Task CreatedAsync(WorkflowInstanceCreatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
        }

        public async override Task UpdatedAsync(WorkflowInstanceUpdatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
        }

        public override Task DeletedAsync(WorkflowInstanceDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowInstanceRecord.WorkflowInstanceId);
            return Task.CompletedTask;
        }

        private async Task UpdateRouteEntriesAsync(WorkflowInstanceContext context)
        {
            var workflowInstanceRecord = context.WorkflowInstanceRecord;
            var workflowDefinitionRecord = await _workflowDefinitionStore.GetAsync(workflowInstanceRecord.WorkflowDefinitionId);
            var entries = WorkflowInstanceRouteEntries.GetWorkflowInstanceRoutesEntries(workflowDefinitionRecord, context.WorkflowInstanceRecord, _activityLibrary);

            _workflowRouteEntries.AddEntries(entries);
        }
    }
}
