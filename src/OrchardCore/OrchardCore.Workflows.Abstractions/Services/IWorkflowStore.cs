using System;
using System.Collections.Generic;
using System.Linq;
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
        Task<IEnumerable<Workflow>> ListAsync(string workflowTypeId, string activityName, string correlationId = null, bool isAlwaysCorrelated = false);
        Task<Workflow> GetAsync(long id);
        Task<Workflow> GetAsync(string uid);
        Task<IEnumerable<Workflow>> GetAsync(IEnumerable<long> ids);
        Task<IEnumerable<Workflow>> GetAsync(IEnumerable<string> uids);
        Task SaveAsync(Workflow workflow);
        Task DeleteAsync(Workflow workflow);
    }

    public static class WorkflowStoreExtensions
    {
        [Obsolete("This method will be removed in a future version, use the method accepting a collection of long ids.", false)]
        public static Task<IEnumerable<Workflow>> GetAsync(this IWorkflowStore store, IEnumerable<int> ids) =>
            store.GetAsync(ids.Select(id => Convert.ToInt64(id)));
    }
}
