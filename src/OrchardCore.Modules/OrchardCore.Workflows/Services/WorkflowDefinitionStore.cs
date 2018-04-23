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
    public class WorkflowDefinitionStore : IWorkflowTypeStore
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowTypeEventHandler> _handlers;
        private readonly ILogger<WorkflowDefinitionStore> _logger;

        public WorkflowDefinitionStore(ISession session, IEnumerable<IWorkflowTypeEventHandler> handlers, ILogger<WorkflowDefinitionStore> logger)
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

        public async Task<WorkflowType> GetAsync(string workflowDefinitionId)
        {
            return await _session.Query<WorkflowType, WorkflowDefinitionIndex>(x => x.WorkflowDefinitionId == workflowDefinitionId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WorkflowType>> ListAsync()
        {
            return await _session.Query<WorkflowType, WorkflowDefinitionIndex>().ListAsync();
        }

        public async Task<IList<WorkflowType>> GetByStartActivityAsync(string activityName)
        {
            var query = await _session
                .Query<WorkflowType, WorkflowDefinitionStartActivitiesIndex>(index =>
                    index.StartActivityName == activityName &&
                    index.IsEnabled)
                .ListAsync();

            return query.ToList();
        }

        public async Task SaveAsync(WorkflowType workflowDefinition)
        {
            var isNew = workflowDefinition.Id == 0;
            _session.Save(workflowDefinition);

            if (isNew)
            {
                var context = new WorkflowTypeCreatedContext(workflowDefinition);
                await _handlers.InvokeAsync(async x => await x.CreatedAsync(context), _logger);
            }
            else
            {
                var context = new WorkflowTypeUpdatedContext(workflowDefinition);
                await _handlers.InvokeAsync(async x => await x.UpdatedAsync(context), _logger);
            }
        }

        public async Task DeleteAsync(WorkflowType workflowDefinition)
        {
            // TODO: Remove this when versioning is implemented.

            // Delete workflow instances first.
            var workflowInstances = await _session.Query<Workflow, WorkflowInstanceIndex>(x => x.WorkflowDefinitionId == workflowDefinition.WorkflowTypeId).ListAsync();

            foreach (var workflowInstance in workflowInstances)
            {
                _session.Delete(workflowInstance);
            }

            // Then delete the workflow definition.
            _session.Delete(workflowDefinition);
            var context = new WorkflowTypeDeletedContext(workflowDefinition);
            await _handlers.InvokeAsync(async x => await x.DeletedAsync(context), _logger);
        }
    }
}
