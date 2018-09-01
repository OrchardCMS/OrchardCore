using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Specifications;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowStore : IWorkflowStore
    {
        private readonly ISession _session;
        private readonly IEnumerable<IWorkflowHandler> _handlers;
        private readonly ILogger<WorkflowStore> _logger;

        public WorkflowStore(ISession session, IEnumerable<IWorkflowHandler> handlers, ILogger<WorkflowStore> logger)
        {
            _handlers = handlers;
            _session = session;
            _logger = logger;
        }

        public Task<int> CountAsync()
        {
            return _session.Query<Workflow>().CountAsync();
        }

        public Task<int> CountAsync<TIndex>(Specification<TIndex> specification) where TIndex : class, IIndex
        {
            return _session.Query<Workflow, TIndex>(specification.PredicateExpression).CountAsync();
        }

        public async Task<IEnumerable<Workflow>> ListAsync<TIndex>(Specification<TIndex> specification, int? skip = null, int? take = null) where TIndex : class, IIndex
        {
            var query = _session.Query<Workflow, TIndex>();

            if (specification.PredicateExpression != null)
            {
                query = query.Where(specification.PredicateExpression);
            }

            if (specification.OrderByExpression != null)
            {
                query = query.OrderBy(specification.OrderByExpression);
            }
            else if (specification.OrderByDescendingExpression != null)
            {
                query = query.OrderByDescending(specification.OrderByDescendingExpression);
            }

            if (skip != null)
            {
                query = (IQuery<Workflow, TIndex>)query.Skip(skip.Value);
            }

            if (take != null)
            {
                query = (IQuery<Workflow, TIndex>)query.Take(take.Value);
            }

            return await query.ListAsync();
        }

        public Task<Workflow> GetAsync<TIndex>(Specification<TIndex> specification) where TIndex : class, IIndex
        {
            return _session.Query<Workflow, TIndex>(specification.PredicateExpression).FirstOrDefaultAsync();
        }

        public Task<Workflow> GetAsync(int id)
        {
            return _session.GetAsync<Workflow>(id);
        }

        public Task<IEnumerable<Workflow>> GetAsync(IEnumerable<int> ids)
        {
            return _session.GetAsync<Workflow>(ids.ToArray());
        }

        public async Task<IEnumerable<Workflow>> ListPendingWorkflowsAsync(string activityName, string correlationId = null)
        {
            var query = await _session
                .QueryIndex<WorkflowBlockingActivitiesIndex>(index =>
                    index.ActivityName == activityName &&
                    index.WorkflowCorrelationId == (correlationId ?? ""))
                .ListAsync();

            var pendingWorkflowIndexes = query.ToList();
            var pendingWorkflowIds = pendingWorkflowIndexes.Select(x => x.WorkflowId).Distinct().ToArray();
            var pendingWorkflows = await ListAsync(new ManyWorkflowsSpecification(pendingWorkflowIds));

            return pendingWorkflows.ToList();
        }

        public async Task SaveAsync(Workflow workflow)
        {
            var isNew = workflow.Id == 0;
            _session.Save(workflow);

            if (isNew)
            {
                var context = new WorkflowCreatedContext(workflow);
                await _handlers.InvokeAsync(async x => await x.CreatedAsync(context), _logger);
            }
            else
            {
                var context = new WorkflowUpdatedContext(workflow);
                await _handlers.InvokeAsync(async x => await x.UpdatedAsync(context), _logger);
            }
        }

        public async Task DeleteAsync(Workflow workflow)
        {
            _session.Delete(workflow);

            var context = new WorkflowDeletedContext(workflow);
            await _handlers.InvokeAsync(async x => await x.DeletedAsync(context), _logger);
        }
    }
}
