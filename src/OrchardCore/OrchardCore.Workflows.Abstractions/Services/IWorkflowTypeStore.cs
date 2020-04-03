using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowTypeStore
    {
        Task<WorkflowType> GetAsync(int id);
        Task<WorkflowType> GetAsync(string uid);
        Task<IEnumerable<WorkflowType>> GetAsync(IEnumerable<int> ids);
        Task<IEnumerable<WorkflowType>> ListAsync();
        Task<IList<WorkflowType>> GetByStartActivityAsync(string activityName);
        Task SaveAsync(WorkflowType workflowType);
        Task DeleteAsync(WorkflowType workflowType);
    }
}
