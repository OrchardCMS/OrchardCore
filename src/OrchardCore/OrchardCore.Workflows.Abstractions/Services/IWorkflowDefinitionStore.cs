using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowDefinitionStore
    {
        Task<WorkflowDefinition> GetAsync(int id);
        Task<WorkflowDefinition> GetAsync(string uid);
        Task<IEnumerable<WorkflowDefinition>> GetAsync(IEnumerable<int> ids);
        Task<IEnumerable<WorkflowDefinition>> ListAsync();
        Task<IList<WorkflowDefinition>> GetByStartActivityAsync(string activityName);
        Task SaveAsync(WorkflowDefinition workflowDefinition);
        Task DeleteAsync(WorkflowDefinition workflowDefinition);
    }
}
