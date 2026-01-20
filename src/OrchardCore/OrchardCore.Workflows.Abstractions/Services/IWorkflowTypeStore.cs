using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowTypeStore
    {
        Task<WorkflowType> GetAsync(long id);
        Task<WorkflowType> GetAsync(string uid);
        Task<IEnumerable<WorkflowType>> GetAsync(IEnumerable<long> ids);
        Task<IEnumerable<WorkflowType>> ListAsync();
        Task<IEnumerable<WorkflowType>> GetByStartActivityAsync(string activityName);
        Task SaveAsync(WorkflowType workflowType);
        Task DeleteAsync(WorkflowType workflowType);
    }

    public static class WorkflowTypeStoreExtensions
    {
        [Obsolete("This method will be removed in a future version, use the method accepting a collection of long ids.", false)]
        public static Task<IEnumerable<WorkflowType>> GetAsync(this IWorkflowTypeStore store, IEnumerable<int> ids) =>
            store.GetAsync(ids.Select(id => Convert.ToInt64(id)));
    }
}
