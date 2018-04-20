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
    public class WorkflowInstanceStore : IWorkflowInstanceStore
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowInstanceHandler> _handlers;
        private readonly ILogger<WorkflowInstanceStore> _logger;

        public WorkflowInstanceStore(ISession session, IEnumerable<IWorkflowInstanceHandler> handlers, ILogger<WorkflowInstanceStore> logger)
        {
            _handlers = handlers;
            _session = session;
            _logger = logger;
        }

        public Task<int> CountAsync()
        {
            return _session.Query<WorkflowInstance>().CountAsync();
        }

        public Task<IEnumerable<WorkflowInstance>> ListAsync(int? skip = null, int? take = null)
        {
            var query = (IQuery<WorkflowInstance>)_session.Query<WorkflowInstance, WorkflowInstanceIndex>().OrderByDescending(x => x.CreatedUtc);

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

        public Task<IEnumerable<WorkflowInstance>> ListAsync(IEnumerable<string> workflowDefinitionUids)
        {
            return _session.Query<WorkflowInstance, WorkflowInstanceIndex>(x => x.WorkflowDefinitionId.IsIn(workflowDefinitionUids)).ListAsync();
        }

        public Task<WorkflowInstance> GetAsync(int id)
        {
            return _session.GetAsync<WorkflowInstance>(id);
        }

        public Task<WorkflowInstance> GetAsync(string uid)
        {
            return _session.Query<WorkflowInstance, WorkflowInstanceBlockingActivitiesIndex>(x => x.WorkflowInstanceId == uid).FirstOrDefaultAsync();
        }

        public Task<IEnumerable<WorkflowInstance>> GetAsync(IEnumerable<string> uids)
        {
            var uidList = uids.ToList();
            return _session.Query<WorkflowInstance, WorkflowInstanceBlockingActivitiesIndex>(x => x.WorkflowInstanceCorrelationId.IsIn(uidList)).ListAsync();
        }

        public Task<IEnumerable<WorkflowInstance>> GetAsync(IEnumerable<int> ids)
        {
            return _session.GetAsync<WorkflowInstance>(ids.ToArray());
        }

        public async Task<IEnumerable<WorkflowInstance>> ListAsync(string workflowDefinitionId, IEnumerable<string> blockingActivityIds)
        {
            var query = await _session
                .Query<WorkflowInstance, WorkflowInstanceBlockingActivitiesIndex>(index =>
                    index.WorkflowDefinitionId == workflowDefinitionId &&
                    index.ActivityId.IsIn(blockingActivityIds)
                ).ListAsync();

            return query.ToList();
        }

        public async Task<IEnumerable<WorkflowInstance>> ListAsync(string workflowDefinitionId, string activityName, string correlationId = null)
        {
            var query = await _session
                .Query<WorkflowInstance, WorkflowInstanceBlockingActivitiesIndex>(index =>
                    index.WorkflowDefinitionId == workflowDefinitionId &&
                    index.ActivityName == activityName &&
                    index.WorkflowInstanceCorrelationId == (correlationId ?? ""))
                .ListAsync();

            return query.ToList();
        }

        public async Task<IEnumerable<WorkflowInstance>> ListAsync(string activityName, string correlationId = null)
        {
            var query = await _session
                .QueryIndex<WorkflowInstanceBlockingActivitiesIndex>(index =>
                    index.ActivityName == activityName &&
                    index.WorkflowInstanceCorrelationId == (correlationId ?? ""))
                .ListAsync();

            var pendingWorkflowInstanceIndexes = query.ToList();
            var pendingWorkflowInstanceIds = pendingWorkflowInstanceIndexes.Select(x => x.WorkflowInstanceId).Distinct().ToArray();
            var pendingWorkflowInstances = await _session.Query<WorkflowInstance, WorkflowInstanceIndex>(x => x.WorkflowInstanceId.IsIn(pendingWorkflowInstanceIds)).ListAsync();

            return pendingWorkflowInstances.ToList();
        }

        public async Task SaveAsync(WorkflowInstance workflowInstance)
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

        public async Task DeleteAsync(WorkflowInstance workflowInstance)
        {
            _session.Delete(workflowInstance);

            var context = new WorkflowInstanceDeletedContext(workflowInstance);
            await _handlers.InvokeAsync(async x => await x.DeletedAsync(context), _logger);
        }
    }
}
