using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowDefinitionHandler
    {
        Task CreatedAsync(WorkflowDefinitionCreatedContext context);
        Task UpdatedAsync(WorkflowDefinitionUpdatedContext context);
        Task DeletedAsync(WorkflowDefinitionDeletedContext context);
    }
}
