using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowInstanceHandler> _handlers;
        private readonly ILogger<WorkflowInstanceRepository> _logger;

        public WorkflowInstanceRepository(ISession session, IEnumerable<IWorkflowInstanceHandler> handlers, ILogger<WorkflowInstanceRepository> logger)
        {
            _handlers = handlers;
            _session = session;
            _logger = logger;
        }

        public Task<int> CountAsync()
        {
            return _session.Query<WorkflowInstanceRecord>().CountAsync();
        }

        public Task<IEnumerable<WorkflowInstanceRecord>> ListAsync(int? skip = null, int? take = null)
        {
            var query = _session.Query<WorkflowInstanceRecord>();

            if (skip != null)
            {
                query = query.Skip(skip.Value);
            }

            if (take != null)
            {
                query = query.Take(take.Value);
            }

            return query.ListAsync();
        }

        public Task<IEnumerable<WorkflowInstanceRecord>> ListByWorkflowDefinitionsAsync(IEnumerable<int> workflowDefinitionIds)
        {
            return _session.Query<WorkflowInstanceRecord, WorkflowInstanceIndex>(x => x.WorkflowDefinitionId.IsIn(workflowDefinitionIds)).ListAsync();
        }

        public Task<WorkflowInstanceRecord> GetAsync(int id)
        {
            return _session.GetAsync<WorkflowInstanceRecord>(id);
        }

        public Task<WorkflowInstanceRecord> GetAsync(string uid)
        {
            return _session.Query<WorkflowInstanceRecord, WorkflowInstanceByAwaitingActivitiesIndex>(x => x.WorkflowInstanceUid == uid).FirstOrDefaultAsync();
        }

        public Task<IEnumerable<WorkflowInstanceRecord>> GetAsync(IEnumerable<string> uids)
        {
            var uidList = uids.ToList();
            return _session.Query<WorkflowInstanceRecord, WorkflowInstanceByAwaitingActivitiesIndex>(x => x.WorkflowInstanceCorrelationId.IsIn(uidList)).ListAsync();
        }

        public Task<IEnumerable<WorkflowInstanceRecord>> GetAsync(IEnumerable<int> ids)
        {
            return _session.GetAsync<WorkflowInstanceRecord>(ids.ToArray());
        }

        public async Task<IEnumerable<WorkflowInstanceRecord>> GetWaitingWorkflowInstancesAsync(string activityName, string correlationId = null)
        {
            var query = await _session
                .QueryIndex<WorkflowInstanceByAwaitingActivitiesIndex>(index =>
                    index.ActivityName == activityName &&
                    index.WorkflowInstanceCorrelationId == (correlationId ?? "")) // TODO: Can't compare NULL == NULL for some reason, so converting to empty string.
                .ListAsync();

            var pendingWorkflowInstanceIndexes = query.ToList();
            var pendingWorkflowInstanceIds = pendingWorkflowInstanceIndexes.Select(x => x.WorkflowInstanceId).Distinct().ToArray();
            var pendingWorkflowInstances = await _session.GetAsync<WorkflowInstanceRecord>(pendingWorkflowInstanceIds);

            return pendingWorkflowInstances.ToList();
        }

        public async Task SaveAsync(WorkflowInstanceRecord workflowInstance)
        {
            var isNew = workflowInstance.Id == 0;
            _session.Save(workflowInstance);

            if (isNew)
            {
                var context = new WorkflowInstanceCreatedContext(workflowInstance);
                await _handlers.InvokeAsync(async x => await x.CreatedAsync(context), _logger);
            }
            else
            {
                var context = new WorkflowInstanceUpdatedContext(workflowInstance);
                await _handlers.InvokeAsync(async x => await x.UpdatedAsync(context), _logger);
            }
        }

        public async Task DeleteAsync(WorkflowInstanceRecord workflowInstance)
        {
            _session.Delete(workflowInstance);

            var context = new WorkflowInstanceDeletedContext(workflowInstance);
            await _handlers.InvokeAsync(async x => await x.DeletedAsync(context), _logger);
        }
    }
}
