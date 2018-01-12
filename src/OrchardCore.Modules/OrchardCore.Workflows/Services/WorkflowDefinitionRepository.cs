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
    public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowDefinitionHandler> _handlers;
        readonly ILogger<WorkflowDefinitionRepository> _logger;

        public WorkflowDefinitionRepository(ISession session, IEnumerable<IWorkflowDefinitionHandler> handlers, ILogger<WorkflowDefinitionRepository> logger)
        {
            _session = session;
            _handlers = handlers;
            _logger = logger;
        }

        public Task<WorkflowDefinitionRecord> GetAsync(int id)
        {
            return _session.GetAsync<WorkflowDefinitionRecord>(id);
        }

        public Task<IEnumerable<WorkflowDefinitionRecord>> ListAsync()
        {
            return _session.Query<WorkflowDefinitionRecord, WorkflowDefinitionsIndex>().ListAsync();
        }

        public Task<WorkflowDefinitionRecord> GetWorkflowDefinitionAsync(int id)
        {
            return _session.GetAsync<WorkflowDefinitionRecord>(id);
        }

        public Task<IEnumerable<WorkflowDefinitionRecord>> GetWorkflowDefinitionsAsync(IEnumerable<int> ids)
        {
            return _session.GetAsync<WorkflowDefinitionRecord>(ids.ToArray());
        }

        public async Task<IList<WorkflowDefinitionRecord>> GetWorkflowDefinitionsByStartActivityAsync(string activityName)
        {
            var query = await _session
                .Query<WorkflowDefinitionRecord, WorkflowDefinitionStartActivitiesIndex>(index =>
                    index.StartActivityName == activityName &&
                    index.IsEnabled)
                .ListAsync();

            return query.ToList();
        }

        public async Task SaveAsync(WorkflowDefinitionRecord workflowDefinition)
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

        public async Task DeleteAsync(WorkflowDefinitionRecord workflowDefinition)
        {
            // TODO: Remove this when versioning is implemented.

            // Delete workflow instances first.
            var workflowInstances = await _session.Query<WorkflowInstanceRecord, WorkflowInstanceIndex>(x => x.WorkflowDefinitionId == workflowDefinition.Id).ListAsync();

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
