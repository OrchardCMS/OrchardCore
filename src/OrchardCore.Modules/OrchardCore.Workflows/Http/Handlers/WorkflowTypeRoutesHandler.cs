using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Handlers
{
    internal class WorkflowTypeRoutesHandler : WorkflowTypeHandlerBase
    {
        private readonly IWorkflowTypeRouteEntries _workflowRouteEntries;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowTypeRoutesHandler(IWorkflowTypeRouteEntries workflowRouteEntries, IActivityLibrary activityLibrary)
        {
            _workflowRouteEntries = workflowRouteEntries;
            _activityLibrary = activityLibrary;
        }

        public override Task CreatedAsync(WorkflowTypeCreatedContext context)
        {
            return UpdateRouteEntriesAsync(context);
        }

        public override Task UpdatedAsync(WorkflowTypeUpdatedContext context)
        {
            return UpdateRouteEntriesAsync(context);
        }

        public override Task DeletedAsync(WorkflowTypeDeletedContext context)
        {
            return _workflowRouteEntries.RemoveEntriesAsync(context.WorkflowType.Id.ToString());
        }

        private Task UpdateRouteEntriesAsync(WorkflowTypeContext context)
        {
            var entries = WorkflowTypeRouteEntries.GetWorkflowTypeRoutesEntries(context.WorkflowType, _activityLibrary);
            return _workflowRouteEntries.AddEntriesAsync(entries);
        }
    }
}
