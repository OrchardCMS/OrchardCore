using System.Collections.Generic;
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

        //public Task<int> CountAsync(string workflowTypeVersionId = null)
        //{
        //    return FilterByWorkflowTypeVersionId(_session.Query<Workflow, WorkflowIndex>(), workflowTypeVersionId).CountAsync();
        //}

        public async Task<bool> HasHaltedInstanceAsync(string workflowTypeVersionId)
        {
            return (await _session.Query<Workflow, WorkflowIndex>(x => x.WorkflowTypeVersionId == workflowTypeVersionId)
                .With<WorkflowBlockingActivitiesIndex>().FirstOrDefaultAsync()) != null;
        }

        //public Task<IEnumerable<Workflow>> ListAsync(string workflowTypeVersionId = null, int? skip = null, int? take = null)
        //{
        //    var query = (IQuery<Workflow>)FilterByWorkflowTypeVersionId(_session.Query<Workflow, WorkflowIndex>(), workflowTypeVersionId)
        //        .OrderByDescending(x => x.CreatedUtc);

        //    if (skip != null)
        //    {
        //        query = query.Skip(skip.Value);
        //    }

        //    if (take != null)
        //    {
        //        query = query.Take(take.Value);
        //    }

        //    return query.ListAsync();
        //}

        //public Task<IEnumerable<Workflow>> ListAsync(IEnumerable<string> workflowTypeVersionIds)
        //{
        //    return _session.Query<Workflow, WorkflowIndex>(x => x.WorkflowTypeVersionId.IsIn(workflowTypeVersionIds)).ListAsync();
        //}

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

        //public Task<IEnumerable<Workflow>> GetAsync(IEnumerable<long> ids)
        //{
        //    return _session.GetAsync<Workflow>(ids.ToArray());
        //}

        public Task<IEnumerable<Workflow>> ListAsync(string workflowTypeVersionId, IEnumerable<string> blockingActivityIds)
        {
            return _session
                .Query<Workflow, WorkflowIndex>(index => index.WorkflowTypeVersionId == workflowTypeVersionId)
                .With<WorkflowBlockingActivitiesIndex>(index => index.ActivityId.IsIn(blockingActivityIds))
                .ListAsync();
        }

        public Task<IEnumerable<Workflow>> ListAsync(string workflowTypeVersionId, string activityName, string correlationId = null, bool isAlwaysCorrelated = false)
        {
            var query = _session.Query<Workflow, WorkflowIndex>(index => index.WorkflowTypeVersionId == workflowTypeVersionId)
            .With<WorkflowBlockingActivitiesIndex>(index => index.ActivityName == activityName);
            if (!isAlwaysCorrelated)
            {
                query = query.Where(index => index.WorkflowCorrelationId == (correlationId ?? ""));
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
            // 构建新流程
            var isNew = workflow.Id == 0;
            await _session.SaveAsync(workflow);

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

        //private static IQuery<Workflow, WorkflowIndex> FilterByWorkflowTypeVersionId(IQuery<Workflow, WorkflowIndex> query, string workflowTypeVersionId)
        //{
        //    if (workflowTypeVersionId != null)
        //    {
        //        query = query.Where(x => x.WorkflowTypeVersionId == workflowTypeVersionId);
        //    }

        //    return query;
        //}
    }
}
