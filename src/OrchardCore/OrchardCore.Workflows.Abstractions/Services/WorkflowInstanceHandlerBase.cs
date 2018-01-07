using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowInstanceHandlerBase : IWorkflowInstanceHandler
    {
        public virtual Task CreatedAsync(WorkflowInstanceCreatedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UpdatedAsync(WorkflowInstanceUpdatedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task DeletedAsync(WorkflowInstanceDeletedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
