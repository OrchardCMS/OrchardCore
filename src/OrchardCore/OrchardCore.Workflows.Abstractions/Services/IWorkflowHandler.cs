using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowHandler
    {
        Task CreatedAsync(WorkflowCreatedContext context);
        Task UpdatedAsync(WorkflowUpdatedContext context);
        Task DeletedAsync(WorkflowDeletedContext context);
    }
}
