using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Specifications;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowStore
    {
        Task<int> CountAsync();
        Task<int> CountAsync<TIndex>(Specification<TIndex> specification) where TIndex : class, IIndex;
        Task<IEnumerable<Workflow>> ListAsync<TIndex>(Specification<TIndex> specification, int? skip = null, int? take = null) where TIndex : class, IIndex;
        Task<IEnumerable<Workflow>> ListPendingWorkflowsAsync(string activityName, string correlationId = null);
        Task<Workflow> GetAsync(int id);
        Task<Workflow> GetAsync<TIndex>(Specification<TIndex> specification) where TIndex : class, IIndex;
        Task SaveAsync(Workflow workflow);
        Task DeleteAsync(Workflow workflow);
    }
}
