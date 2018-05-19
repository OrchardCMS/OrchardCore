using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowTypeHandlerBase : IWorkflowTypeEventHandler
    {
        public virtual Task CreatedAsync(WorkflowTypeCreatedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task DeletedAsync(WorkflowTypeDeletedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UpdatedAsync(WorkflowTypeUpdatedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
