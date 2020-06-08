using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowStore
    {
        Task<int> CountAsync(string workflowTypeId = null);
        Task<bool> HasHaltedInstanceAsync(string workflowTypeId);
        Task<IEnumerable<Workflow>> ListAsync(string workflowTypeId = null, int? skip = null, int? take = null);
        Task<IEnumerable<Workflow>> ListAsync(IEnumerable<string> workflowTypeIds);
        Task<IEnumerable<Workflow>> ListAsync(string workflowTypeId, IEnumerable<string> blockingActivityIds);
        Task<IEnumerable<Workflow>> ListByActivityNameAsync(string activityName, string correlationId = null, bool isAlwaysCorrelated = false);
        Task<IEnumerable<Workflow>> ListAsync(string workflowTypeId, string activityName, string correlationId = null);
        Task<Workflow> GetAsync(int id);
        Task<Workflow> GetAsync(string uid);
        Task<IEnumerable<Workflow>> GetAsync(IEnumerable<int> ids);
        Task<IEnumerable<Workflow>> GetAsync(IEnumerable<string> uids);
        Task SaveAsync(Workflow workflow);
        Task DeleteAsync(Workflow workflow);
    }
}
