using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowTypeStore : IWorkflowTypeStore
    {
        private readonly YesSql.ISession _session;
        private readonly IEnumerable<IWorkflowTypeEventHandler> _handlers;
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly IWorkflowTypeIdGenerator _idGenerator;
        private readonly IHttpContextAccessor _contextAccessor;
        public WorkflowTypeStore(YesSql.ISession session, IEnumerable<IWorkflowTypeEventHandler> handlers, ILogger<WorkflowTypeStore> logger, IWorkflowTypeIdGenerator idGenerator, IClock clock, IHttpContextAccessor contextAccessor)
        {
            _session = session;
            _handlers = handlers;
            _logger = logger;
            _idGenerator = idGenerator;
            _clock = clock;
            _contextAccessor = contextAccessor;
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
            return _session.Query<WorkflowType, WorkflowTypeIndex>(x => x.WorkflowTypeId == workflowTypeId && x.Latest == true)
                .FirstOrDefaultAsync();
        }

        public Task<WorkflowType> GetByVersionAsync(string workflowTypeVersionId)
        {
            return _session.Query<WorkflowType, WorkflowTypeIndex>(x => x.WorkflowTypeVersionId == workflowTypeVersionId)
                .FirstOrDefaultAsync();
        }

        public Task<IEnumerable<WorkflowType>> ListAsync()
        {
            return _session.Query<WorkflowType, WorkflowTypeIndex>(x => x.Latest).ListAsync();
        }

        public async Task<IEnumerable<WorkflowType>> GetByStartActivityAsync(string activityName)
        {
            return await _session
                .Query<WorkflowType, WorkflowTypeStartActivitiesIndex>(index =>
                    index.StartActivityName == activityName &&
                    index.IsEnabled)
                .With<WorkflowTypeIndex>(x => x.Latest)
                .ListAsync();
        }



        public async Task SaveAsync(WorkflowType workflowType, bool newVersion = false)
        {
            var isNew = workflowType.Id == 0;
            if (newVersion)
            {
                var existsedEntity = await GetAsync(workflowType.WorkflowTypeId);
                if (existsedEntity != null)
                {
                    existsedEntity.Latest = false;
                    existsedEntity.ModifiedUtc = _clock.UtcNow;
                    existsedEntity.ModifiedBy = workflowType.ModifiedBy;
                    await _session.SaveAsync(existsedEntity);
                }
                // reset to new entity.
                workflowType = JObject.FromObject(workflowType).ToObject<WorkflowType>();
                workflowType.Id = 0;
                workflowType.WorkflowTypeVersionId = _idGenerator.GenerateVersionUniqueId(workflowType);
            }

            workflowType.DisplayName ??= workflowType.Name;
            workflowType.Latest = true;
            workflowType.ModifiedUtc = _clock.UtcNow;
            workflowType.ModifiedBy = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _session.SaveAsync(workflowType);

            if (isNew)
            {
                workflowType.CreatedUtc = _clock.UtcNow;
                workflowType.CreatedBy = workflowType.ModifiedBy;

                var context = new WorkflowTypeCreatedContext(workflowType);
                await _handlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);
            }
            else
            {

                var context = new WorkflowTypeUpdatedContext(workflowType);
                await _handlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), context, _logger);
            }
        }

        public async Task DeleteAsync(WorkflowType workflowType)
        {
            workflowType.Latest = false;
            workflowType.ModifiedUtc = _clock.UtcNow;
            workflowType.ModifiedBy = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _session.SaveAsync(workflowType);
            var context = new WorkflowTypeDeletedContext(workflowType);
            await _handlers.InvokeAsync((handler, context) => handler.DeletedAsync(context), context, _logger);
        }
    }
}
