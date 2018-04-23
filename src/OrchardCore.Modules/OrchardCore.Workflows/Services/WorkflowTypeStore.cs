using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowTypeStore : IWorkflowTypeStore
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowTypeEventHandler> _handlers;
        private readonly ILogger<WorkflowTypeStore> _logger;

        public WorkflowTypeStore(ISession session, IEnumerable<IWorkflowTypeEventHandler> handlers, ILogger<WorkflowTypeStore> logger)
        {
            _session = session;
            _handlers = handlers;
            _logger = logger;
        }

        public Task<WorkflowType> GetAsync(int id)
        {
            return _session.GetAsync<WorkflowType>(id);
        }

        public async Task<IEnumerable<WorkflowType>> GetAsync(IEnumerable<int> ids)
        {
            return await _session.GetAsync<WorkflowType>(ids.ToArray());
        }

        public async Task<WorkflowType> GetAsync(string workflowTypeId)
        {
            return await _session.Query<WorkflowType, WorkflowTypeIndex>(x => x.WorkflowTypeId == workflowTypeId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WorkflowType>> ListAsync()
        {
            return await _session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync();
        }

        public async Task<IList<WorkflowType>> GetByStartActivityAsync(string activityName)
        {
            var query = await _session
                .Query<WorkflowType, WorkflowTypeStartActivitiesIndex>(index =>
                    index.StartActivityName == activityName &&
                    index.IsEnabled)
                .ListAsync();

            return query.ToList();
        }

        public async Task SaveAsync(WorkflowType workflowType)
        {
            var isNew = workflowType.Id == 0;
            _session.Save(workflowType);

            if (isNew)
            {
                var context = new WorkflowTypeCreatedContext(workflowType);
                await _handlers.InvokeAsync(async x => await x.CreatedAsync(context), _logger);
            }
            else
            {
                var context = new WorkflowTypeUpdatedContext(workflowType);
                await _handlers.InvokeAsync(async x => await x.UpdatedAsync(context), _logger);
            }
        }

        public async Task DeleteAsync(WorkflowType workflowType)
        {
            // TODO: Remove this when versioning is implemented.

            // Delete workflows first.
            var workflows = await _session.Query<Workflow, WorkflowIndex>(x => x.WorkflowTypeId == workflowType.WorkflowTypeId).ListAsync();

            foreach (var workflow in workflows)
            {
                _session.Delete(workflow);
            }

            // Then delete the workflow type.
            _session.Delete(workflowType);
            var context = new WorkflowTypeDeletedContext(workflowType);
            await _handlers.InvokeAsync(async x => await x.DeletedAsync(context), _logger);
        }
    }
}
