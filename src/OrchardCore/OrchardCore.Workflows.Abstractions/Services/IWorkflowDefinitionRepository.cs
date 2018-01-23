using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowDefinitionRepository
    {
        Task<WorkflowDefinitionRecord> GetAsync(int id);
        Task<IEnumerable<WorkflowDefinitionRecord>> GetAsync(IEnumerable<int> ids);
        Task<IEnumerable<WorkflowDefinitionRecord>> ListAsync();
        Task<IList<WorkflowDefinitionRecord>> GetByStartActivityAsync(string activityName);
        Task SaveAsync(WorkflowDefinitionRecord workflowDefinition);
        Task DeleteAsync(WorkflowDefinitionRecord workflowDefinition);
    }
}
