using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowStore
    {
        //Task<int> CountAsync(string workflowTypeVersionId = null);
        Task<bool> HasHaltedInstanceAsync(string workflowTypeVersionId);
        //Task<IEnumerable<Workflow>> ListAsync(string workflowTypeVersionId = null, int? skip = null, int? take = null);
        //Task<IEnumerable<Workflow>> ListAsync(IEnumerable<string> workflowTypeVersiIds);
        Task<IEnumerable<Workflow>> ListAsync(string workflowTypeVersionId, IEnumerable<string> blockingActivityIds);
        Task<IEnumerable<Workflow>> ListByActivityNameAsync(string activityName, string correlationId = null, bool isAlwaysCorrelated = false);
        Task<IEnumerable<Workflow>> ListAsync(string workflowTypeVersionId, string activityName, string correlationId = null, bool isAlwaysCorrelated = false);
        Task<Workflow> GetAsync(long id);
        Task<Workflow> GetAsync(string workflowId);
        //Task<IEnumerable<Workflow>> GetAsync(IEnumerable<long> ids);
        Task<IEnumerable<Workflow>> GetAsync(IEnumerable<string> workflowIds);
        Task SaveAsync(Workflow workflow);
        Task DeleteAsync(Workflow workflow);
    }
}
