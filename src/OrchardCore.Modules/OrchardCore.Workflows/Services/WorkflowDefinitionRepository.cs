using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        private readonly ISession _session;

        public WorkflowDefinitionRepository(ISession session)
        {
            _session = session;
        }

        public Task<WorkflowDefinitionRecord> GetWorkflowDefinitionAsync(int id)
        {
            return _session.GetAsync<WorkflowDefinitionRecord>(id);
        }

        public async Task<IList<WorkflowDefinitionRecord>> GetWorkflowDefinitionsByStartActivityAsync(string activityName)
        {
            var query = await _session
                .Query<WorkflowDefinitionRecord, WorkflowDefinitionByStartActivityIndex>(index =>
                    index.HasStart &&
                    index.StartActivityName == activityName &&
                    index.IsEnabled)
                .ListAsync();

            return query.ToList();
        }
    }
}
