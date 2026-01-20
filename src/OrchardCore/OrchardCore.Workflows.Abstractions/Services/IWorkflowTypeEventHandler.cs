using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowTypeEventHandler
    {
        Task CreatedAsync(WorkflowTypeCreatedContext context);
        Task UpdatedAsync(WorkflowTypeUpdatedContext context);
        Task DeletedAsync(WorkflowTypeDeletedContext context);
    }
}
