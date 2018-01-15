using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowDefinitionHandlerBase : IWorkflowDefinitionEventHandler
    {
        public virtual Task CreatedAsync(WorkflowDefinitionCreatedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task DeletedAsync(WorkflowDefinitionDeletedContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task UpdatedAsync(WorkflowDefinitionUpdatedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
