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
        private readonly ILogger _logger;

        public WorkflowTypeStore(ISession session, IEnumerable<IWorkflowTypeEventHandler> handlers, ILogger<WorkflowTypeStore> logger)
        {
            _session = session;
            _handlers = handlers;
            _logger = logger;
        }

        public Task<WorkflowType> GetAsync(long id)
        {
            return _session.GetAsync<WorkflowType>(id);
        }

        public Task<IEnumerable<WorkflowType>> GetAsync(IEnumerable<long> ids)
        {
            return _session.GetAsync<WorkflowType>(ids.ToArray());
        }

        public Task<WorkflowType> GetAsync(string workflowTypeId)
        {
            return _session.Query<WorkflowType, WorkflowTypeIndex>(x => x.WorkflowTypeId == workflowTypeId).FirstOrDefaultAsync();
        }

        public Task<IEnumerable<WorkflowType>> ListAsync()
        {
            return _session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync();
        }

        public Task<IEnumerable<WorkflowType>> GetByStartActivityAsync(string activityName)
        {
            return _session
                .Query<WorkflowType, WorkflowTypeStartActivitiesIndex>(index =>
                    index.StartActivityName == activityName &&
                    index.IsEnabled)
                .ListAsync();
        }

        public Task SaveAsync(WorkflowType workflowType)
        {
            var isNew = workflowType.Id == 0;
            _session.Save(workflowType);

            if (isNew)
            {
                var context = new WorkflowTypeCreatedContext(workflowType);
                return _handlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);
            }
            else
            {
                var context = new WorkflowTypeUpdatedContext(workflowType);
                return _handlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), context, _logger);
            }
        }

        public async Task DeleteAsync(WorkflowType workflowType)
        {
            // Delete workflows first.
            var workflows = await _session.Query<Workflow, WorkflowIndex>(x => x.WorkflowTypeId == workflowType.WorkflowTypeId).ListAsync();

            foreach (var workflow in workflows)
            {
                _session.Delete(workflow);
            }

            // Then delete the workflow type.
            _session.Delete(workflowType);
            var context = new WorkflowTypeDeletedContext(workflowType);
            await _handlers.InvokeAsync((handler, context) => handler.DeletedAsync(context), context, _logger);
        }
    }
}
