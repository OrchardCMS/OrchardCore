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

        public override Task CreatedAsync(WorkflowInstanceCreatedContext context)
        {
            UpdateRouteEntries(context);
            UpdatePathEntries(context);
            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(WorkflowInstanceUpdatedContext context)
        {
            UpdateRouteEntries(context);
            UpdatePathEntries(context);
            return Task.CompletedTask;
        }

        public override Task DeletedAsync(WorkflowInstanceDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowInstance.Uid);
            _workflowPathEntries.RemoveEntries(context.WorkflowInstance.Uid);
            return Task.CompletedTask;
        }

        private void UpdateRouteEntries(WorkflowInstanceContext context)
        {
            var workflowInstance = context.WorkflowInstance;
            var entries = WorkflowInstanceRouteEntries.GetWorkflowInstanceRoutesEntries(context.WorkflowInstance, _activityLibrary);

            _workflowRouteEntries.AddEntries(entries);
        }

        private void UpdatePathEntries(WorkflowInstanceContext context)
        {
            var workflowInstance = context.WorkflowInstance;
            var entries = WorkflowInstancePathEntries.GetWorkflowPathEntries(context.WorkflowInstance, _activityLibrary);

            _workflowPathEntries.AddEntries(entries);
        }
    }
}
