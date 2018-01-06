using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowInstanceRepository
    {
        Task<IEnumerable<WorkflowInstanceRecord>> ListAsync();
        Task<WorkflowInstanceRecord> GetAsync(int id);
        Task<WorkflowInstanceRecord> GetAsync(string uid);
        Task<IEnumerable<WorkflowInstanceRecord>> GetAsync(IEnumerable<int> ids);
        Task<IEnumerable<WorkflowInstanceRecord>> GetAsync(IEnumerable<string> uids);
        Task<IEnumerable<WorkflowInstanceRecord>> GetWaitingWorkflowInstancesAsync(string activityName, string correlationId = null);
        Task SaveAsync(WorkflowInstanceRecord workflowInstance);
        Task DeleteAsync(WorkflowInstanceRecord workflowInstance);
    }

    public static class WorkflowInstanceRepositoryExtensions
    {
        public static Task SaveAsync(this IWorkflowInstanceRepository repository, WorkflowContext workflowContext)
        {
            workflowContext.WorkflowInstance.State = JObject.FromObject(workflowContext.State);
            return repository.SaveAsync(workflowContext.WorkflowInstance);
        }
    }
}
