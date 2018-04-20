using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowInstanceStore
    {
        Task<int> CountAsync();
        Task<IEnumerable<WorkflowInstance>> ListAsync(int? skip = null, int? take = null);
        Task<IEnumerable<WorkflowInstance>> ListAsync(IEnumerable<string> workflowDefinitionIds);
        Task<IEnumerable<WorkflowInstance>> ListAsync(string workflowDefinitionId, IEnumerable<string> blockingActivityIds);
        Task<IEnumerable<WorkflowInstance>> ListAsync(string activityName, string correlationId = null);
        Task<IEnumerable<WorkflowInstance>> ListAsync(string workflowDefinitionId, string activityName, string correlationId = null);
        Task<WorkflowInstance> GetAsync(int id);
        Task<WorkflowInstance> GetAsync(string uid);
        Task<IEnumerable<WorkflowInstance>> GetAsync(IEnumerable<int> ids);
        Task<IEnumerable<WorkflowInstance>> GetAsync(IEnumerable<string> uids);
        Task SaveAsync(WorkflowInstance workflowInstance);
        Task DeleteAsync(WorkflowInstance workflowInstance);
    }
}
