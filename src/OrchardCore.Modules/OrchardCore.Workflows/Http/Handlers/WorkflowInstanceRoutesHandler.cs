using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Handlers
{
    public class WorkflowInstanceRoutesHandler : WorkflowInstanceHandlerBase
    {
        private readonly IWorkflowInstanceRouteEntries _workflowRouteEntries;
        private readonly IWorkflowInstancePathEntries _workflowPathEntries;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowInstanceRoutesHandler(
            IWorkflowInstanceRouteEntries workflowRouteEntries,
            IWorkflowInstancePathEntries workflowPathEntries,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IActivityLibrary activityLibrary
        )
        {
            _workflowRouteEntries = workflowRouteEntries;
            _workflowPathEntries = workflowPathEntries;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _activityLibrary = activityLibrary;
        }

        public async override Task CreatedAsync(WorkflowInstanceCreatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
            await UpdatePathEntriesAsync(context);
        }

        public async override Task UpdatedAsync(WorkflowInstanceUpdatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
            await UpdatePathEntriesAsync(context);
        }

        public override Task DeletedAsync(WorkflowInstanceDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowInstanceRecord.Uid);
            _workflowPathEntries.RemoveEntries(context.WorkflowInstanceRecord.Uid);
            return Task.CompletedTask;
        }

        private async Task UpdateRouteEntriesAsync(WorkflowInstanceContext context)
        {
            var workflowInstanceRecord = context.WorkflowInstanceRecord;
            var workflowDefinitionRecord = await _workflowDefinitionRepository.GetAsync(workflowInstanceRecord.WorkflowDefinitionUid);
            var entries = WorkflowInstanceRouteEntries.GetWorkflowInstanceRoutesEntries(workflowDefinitionRecord, context.WorkflowInstanceRecord, _activityLibrary);

            _workflowRouteEntries.AddEntries(entries);
        }

        private async Task UpdatePathEntriesAsync(WorkflowInstanceContext context)
        {
            var workflowInstanceRecord = context.WorkflowInstanceRecord;
            var workflowDefinitionRecord = await _workflowDefinitionRepository.GetAsync(workflowInstanceRecord.WorkflowDefinitionUid);
            var entries = WorkflowInstancePathEntries.GetWorkflowPathEntries(workflowDefinitionRecord, context.WorkflowInstanceRecord, _activityLibrary);

            _workflowPathEntries.AddEntries(entries);
        }
    }
}
