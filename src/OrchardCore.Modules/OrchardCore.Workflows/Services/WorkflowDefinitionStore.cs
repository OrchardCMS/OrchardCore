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
    public class WorkflowDefinitionStore : IWorkflowDefinitionStore
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowDefinitionEventHandler> _handlers;
        private readonly ILogger<WorkflowDefinitionStore> _logger;

        public WorkflowDefinitionStore(ISession session, IEnumerable<IWorkflowDefinitionEventHandler> handlers, ILogger<WorkflowDefinitionStore> logger)
        {
            _session = session;
            _handlers = handlers;
            _logger = logger;
        }

        public Task<WorkflowDefinition> GetAsync(int id)
        {
            return _session.GetAsync<WorkflowDefinition>(id);
        }

        public async Task<IEnumerable<WorkflowDefinition>> GetAsync(IEnumerable<int> ids)
        {
            return await _session.GetAsync<WorkflowDefinition>(ids.ToArray());
        }

        public async Task<WorkflowDefinition> GetAsync(string workflowDefinitionId)
        {
            return await _session.Query<WorkflowDefinition, WorkflowDefinitionIndex>(x => x.WorkflowDefinitionId == workflowDefinitionId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WorkflowDefinition>> ListAsync()
        {
            return await _session.Query<WorkflowDefinition, WorkflowDefinitionIndex>().ListAsync();
        }

        public async Task<IList<WorkflowDefinition>> GetByStartActivityAsync(string activityName)
        {
            var query = await _session
                .Query<WorkflowDefinition, WorkflowDefinitionStartActivitiesIndex>(index =>
                    index.StartActivityName == activityName &&
                    index.IsEnabled)
                .ListAsync();

            return query.ToList();
        }

        public async Task SaveAsync(WorkflowDefinition workflowDefinition)
        {
            var isNew = workflowDefinition.Id == 0;
            _session.Save(workflowDefinition);

            if (isNew)
            {
                var context = new WorkflowDefinitionCreatedContext(workflowDefinition);
                await _handlers.InvokeAsync(async x => await x.CreatedAsync(context), _logger);
            }
            else
            {
                var context = new WorkflowDefinitionUpdatedContext(workflowDefinition);
                await _handlers.InvokeAsync(async x => await x.UpdatedAsync(context), _logger);
            }
        }

        public async Task DeleteAsync(WorkflowDefinition workflowDefinition)
        {
            // TODO: Remove this when versioning is implemented.

            // Delete workflow instances first.
            var workflowInstances = await _session.Query<WorkflowInstance, WorkflowInstanceIndex>(x => x.WorkflowDefinitionId == workflowDefinition.WorkflowDefinitionId).ListAsync();

            foreach (var workflowInstance in workflowInstances)
            {
                _session.Delete(workflowInstance);
            }

            // Then delete the workflow definition.
            _session.Delete(workflowDefinition);
            var context = new WorkflowDefinitionDeletedContext(workflowDefinition);
            await _handlers.InvokeAsync(async x => await x.DeletedAsync(context), _logger);
        }
    }
}
