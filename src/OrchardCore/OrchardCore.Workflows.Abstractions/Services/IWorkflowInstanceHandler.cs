using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowInstanceHandler
    {
        Task CreatedAsync(WorkflowInstanceCreatedContext context);
        Task UpdatedAsync(WorkflowInstanceUpdatedContext context);
        Task DeletedAsync(WorkflowInstanceDeletedContext context);
    }
}
