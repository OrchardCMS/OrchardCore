using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowInstanceRepository
    {
        Task<int> CountAsync();
        Task<IEnumerable<WorkflowInstanceRecord>> ListAsync(int? skip = null, int? take = null);
        Task<IEnumerable<WorkflowInstanceRecord>> ListByWorkflowDefinitionsAsync(IEnumerable<string> workflowDefinitionUids);
        Task<WorkflowInstanceRecord> GetAsync(int id);
        Task<WorkflowInstanceRecord> GetAsync(string uid);
        Task<IEnumerable<WorkflowInstanceRecord>> GetAsync(IEnumerable<int> ids);
        Task<IEnumerable<WorkflowInstanceRecord>> GetAsync(IEnumerable<string> uids);
        Task<IEnumerable<WorkflowInstanceRecord>> GetWaitingWorkflowInstancesAsync(string activityName, string correlationId = null);
        Task SaveAsync(WorkflowInstanceRecord workflowInstance);
        Task DeleteAsync(WorkflowInstanceRecord workflowInstance);
    }
}
