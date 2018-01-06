using System.Threading.Tasks;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Handlers
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

        public override async Task CreatedAsync(WorkflowInstanceCreatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
            await UpdatePathEntriesAsync(context);
        }

        public override async Task UpdatedAsync(WorkflowInstanceUpdatedContext context)
        {
            await UpdateRouteEntriesAsync(context);
            await UpdatePathEntriesAsync(context);
        }

        public override Task DeletedAsync(WorkflowInstanceDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowInstance.Uid);
            _workflowPathEntries.RemoveEntries(context.WorkflowInstance.Uid);
            return Task.CompletedTask;
        }

        private async Task UpdateRouteEntriesAsync(WorkflowInstanceContext context)
        {
            var workflowInstance = context.WorkflowInstance;
            var workflowDefinition = await _workflowDefinitionRepository.GetAsync(workflowInstance.DefinitionId);
            var entries = WorkflowInstanceRouteEntries.GetWorkflowInstanceRoutesEntries(context.WorkflowInstance, workflowDefinition, _activityLibrary);

            _workflowRouteEntries.AddEntries(entries);
        }

        private async Task UpdatePathEntriesAsync(WorkflowInstanceContext context)
        {
            var workflowInstance = context.WorkflowInstance;
            var workflowDefinition = await _workflowDefinitionRepository.GetAsync(workflowInstance.DefinitionId);
            var entries = WorkflowInstancePathEntries.GetWorkflowPathEntries(context.WorkflowInstance, workflowDefinition, _activityLibrary);

            _workflowPathEntries.AddEntries(entries);
        }
    }
}
