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
    public class WorkflowStore : IWorkflowStore
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowHandler> _handlers;
        private readonly ILogger _logger;

        public WorkflowStore(ISession session, IEnumerable<IWorkflowHandler> handlers, ILogger<WorkflowStore> logger)
        {
            _handlers = handlers;
            _session = session;
            _logger = logger;
        }

        public Task<int> CountAsync(string workflowTypeId = null)
        {
            return FilterByWorkflowTypeId(_session.Query<Workflow, WorkflowIndex>(), workflowTypeId).CountAsync();
        }

        public async Task<bool> HasHaltedInstanceAsync(string workflowTypeId)
        {
            return (await _session.Query<Workflow, WorkflowBlockingActivitiesIndex>(x => x.WorkflowTypeId == workflowTypeId).FirstOrDefaultAsync()) != null;
        }

        public Task<IEnumerable<Workflow>> ListAsync(string workflowTypeId = null, int? skip = null, int? take = null)
        {
            var query = (IQuery<Workflow>)FilterByWorkflowTypeId(_session.Query<Workflow, WorkflowIndex>(), workflowTypeId)
                .OrderByDescending(x => x.CreatedUtc);

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

        public Task<IEnumerable<Workflow>> ListAsync(IEnumerable<string> workflowTypeIds)
        {
            return _session.Query<Workflow, WorkflowIndex>(x => x.WorkflowTypeId.IsIn(workflowTypeIds)).ListAsync();
        }

        public Task<Workflow> GetAsync(long id)
        {
            return _session.GetAsync<Workflow>(id);
        }

        public Task<Workflow> GetAsync(string workflowId)
        {
            return _session.Query<Workflow, WorkflowBlockingActivitiesIndex>(x => x.WorkflowId == workflowId).FirstOrDefaultAsync();
        }

        public Task<IEnumerable<Workflow>> GetAsync(IEnumerable<string> workflowIds)
        {
            return _session.Query<Workflow, WorkflowBlockingActivitiesIndex>(x => x.WorkflowId.IsIn(workflowIds)).ListAsync();
        }

        public Task<IEnumerable<Workflow>> GetAsync(IEnumerable<long> ids)
        {
            return _session.GetAsync<Workflow>(ids.ToArray());
        }

        public Task<IEnumerable<Workflow>> ListAsync(string workflowTypeId, IEnumerable<string> blockingActivityIds)
        {
            return _session
                .Query<Workflow, WorkflowBlockingActivitiesIndex>(index =>
                    index.WorkflowTypeId == workflowTypeId &&
                    index.ActivityId.IsIn(blockingActivityIds))
                .ListAsync();
        }

        public Task<IEnumerable<Workflow>> ListAsync(string workflowTypeId, string activityName, string correlationId = null, bool isAlwaysCorrelated = false)
        {
            var query = _session.Query<Workflow, WorkflowBlockingActivitiesIndex>();

            if (isAlwaysCorrelated)
            {
                query = query.Where(index => index.WorkflowTypeId == workflowTypeId && index.ActivityName == activityName);
            }
            else
            {
                query = query.Where(index => index.WorkflowTypeId == workflowTypeId && index.ActivityName == activityName && index.WorkflowCorrelationId == (correlationId ?? ""));
            }

            return query.ListAsync();
        }

        public Task<IEnumerable<Workflow>> ListByActivityNameAsync(string activityName, string correlationId = null, bool isAlwaysCorrelated = false)
        {
            var query = _session.Query<Workflow, WorkflowBlockingActivitiesIndex>();

            if (isAlwaysCorrelated)
            {
                query = query.Where(index => index.ActivityName == activityName);
            }
            else
            {
                query = query.Where(index => index.ActivityName == activityName && index.WorkflowCorrelationId == (correlationId ?? ""));
            }

            return query.ListAsync();
        }

        public async Task SaveAsync(Workflow workflow)
        {
            var isNew = workflow.Id == 0;
            _session.Save(workflow);

            if (isNew)
            {
                var context = new WorkflowCreatedContext(workflow);
                await _handlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);
            }
            else
            {
                var context = new WorkflowUpdatedContext(workflow);
                await _handlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), context, _logger);
            }

            // Allow an atomic workflow to get up to date data.
            await _session.FlushAsync();
        }

        public Task DeleteAsync(Workflow workflow)
        {
            _session.Delete(workflow);

            var context = new WorkflowDeletedContext(workflow);
            return _handlers.InvokeAsync((handler, context) => handler.DeletedAsync(context), context, _logger);
        }

        private static IQuery<Workflow, WorkflowIndex> FilterByWorkflowTypeId(IQuery<Workflow, WorkflowIndex> query, string workflowTypeId)
        {
            if (workflowTypeId != null)
            {
                query = query.Where(x => x.WorkflowTypeId == workflowTypeId);
            }

            return query;
        }
    }
}
