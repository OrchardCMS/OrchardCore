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
    public class WorkflowInstanceStore : IWorkflowStore
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowHandler> _handlers;
        private readonly ILogger<WorkflowInstanceStore> _logger;

        public WorkflowInstanceStore(ISession session, IEnumerable<IWorkflowHandler> handlers, ILogger<WorkflowInstanceStore> logger)
        {
            _handlers = handlers;
            _session = session;
            _logger = logger;
        }

        public Task<int> CountAsync()
        {
            return _session.Query<Workflow>().CountAsync();
        }

        public Task<IEnumerable<Workflow>> ListAsync(int? skip = null, int? take = null)
        {
            var query = (IQuery<Workflow>)_session.Query<Workflow, WorkflowInstanceIndex>().OrderByDescending(x => x.CreatedUtc);

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

        public Task<IEnumerable<Workflow>> ListAsync(IEnumerable<string> workflowDefinitionUids)
        {
            return _session.Query<Workflow, WorkflowInstanceIndex>(x => x.WorkflowDefinitionId.IsIn(workflowDefinitionUids)).ListAsync();
        }

        public Task<Workflow> GetAsync(int id)
        {
            return _session.GetAsync<Workflow>(id);
        }

        public Task<Workflow> GetAsync(string uid)
        {
            return _session.Query<Workflow, WorkflowInstanceBlockingActivitiesIndex>(x => x.WorkflowInstanceId == uid).FirstOrDefaultAsync();
        }

        public Task<IEnumerable<Workflow>> GetAsync(IEnumerable<string> uids)
        {
            var uidList = uids.ToList();
            return _session.Query<Workflow, WorkflowInstanceBlockingActivitiesIndex>(x => x.WorkflowInstanceCorrelationId.IsIn(uidList)).ListAsync();
        }

        public Task<IEnumerable<Workflow>> GetAsync(IEnumerable<int> ids)
        {
            return _session.GetAsync<Workflow>(ids.ToArray());
        }

        public async Task<IEnumerable<Workflow>> ListAsync(string workflowDefinitionId, IEnumerable<string> blockingActivityIds)
        {
            var query = await _session
                .Query<Workflow, WorkflowInstanceBlockingActivitiesIndex>(index =>
                    index.WorkflowDefinitionId == workflowDefinitionId &&
                    index.ActivityId.IsIn(blockingActivityIds)
                ).ListAsync();

            return query.ToList();
        }

        public async Task<IEnumerable<Workflow>> ListAsync(string workflowDefinitionId, string activityName, string correlationId = null)
        {
            var query = await _session
                .Query<Workflow, WorkflowInstanceBlockingActivitiesIndex>(index =>
                    index.WorkflowDefinitionId == workflowDefinitionId &&
                    index.ActivityName == activityName &&
                    index.WorkflowInstanceCorrelationId == (correlationId ?? ""))
                .ListAsync();

            return query.ToList();
        }

        public async Task<IEnumerable<Workflow>> ListAsync(string activityName, string correlationId = null)
        {
            var query = await _session
                .QueryIndex<WorkflowInstanceBlockingActivitiesIndex>(index =>
                    index.ActivityName == activityName &&
                    index.WorkflowInstanceCorrelationId == (correlationId ?? ""))
                .ListAsync();

            var pendingWorkflowInstanceIndexes = query.ToList();
            var pendingWorkflowInstanceIds = pendingWorkflowInstanceIndexes.Select(x => x.WorkflowInstanceId).Distinct().ToArray();
            var pendingWorkflowInstances = await _session.Query<Workflow, WorkflowInstanceIndex>(x => x.WorkflowInstanceId.IsIn(pendingWorkflowInstanceIds)).ListAsync();

            return pendingWorkflowInstances.ToList();
        }

        public async Task SaveAsync(Workflow workflowInstance)
        {
            var isNew = workflowInstance.Id == 0;
            _session.Save(workflowInstance);

            if (isNew)
            {
                var context = new WorkflowCreatedContext(workflowInstance);
                await _handlers.InvokeAsync(async x => await x.CreatedAsync(context), _logger);
            }
            else
            {
                var context = new WorkflowUpdatedContext(workflowInstance);
                await _handlers.InvokeAsync(async x => await x.UpdatedAsync(context), _logger);
            }
        }

        public async Task DeleteAsync(Workflow workflowInstance)
        {
            _session.Delete(workflowInstance);

            var context = new WorkflowDeletedContext(workflowInstance);
            await _handlers.InvokeAsync(async x => await x.DeletedAsync(context), _logger);
        }
    }
}
