using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowTypeStore
    {
        Task<WorkflowType> GetAsync(long id);
        Task<WorkflowType> GetAsync(string uid);
        Task<WorkflowType> GetByVersionAsync(string workflowTypeVersionId);
        Task<IEnumerable<WorkflowType>> GetAsync(IEnumerable<long> ids);
        Task<IEnumerable<WorkflowType>> ListAsync();
        Task<IEnumerable<WorkflowType>> GetByStartActivityAsync(string activityName);
        Task SaveAsync(WorkflowType workflowType, bool newVersion = false);
        Task DeleteAsync(WorkflowType workflowType);
    }
}
