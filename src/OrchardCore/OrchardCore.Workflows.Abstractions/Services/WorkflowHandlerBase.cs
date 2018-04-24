using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowHandlerBase : IWorkflowHandler
    {
        public virtual Task CreatedAsync(WorkflowCreatedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UpdatedAsync(WorkflowUpdatedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task DeletedAsync(WorkflowDeletedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
