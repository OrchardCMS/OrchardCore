using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Services;

public class WorkflowTypeStore : IWorkflowTypeStore
{
    private readonly ISession _session;
    private readonly IWorkflowTypeVersionStore _versionStore;
    private readonly IWorkflowExecutionJournal _journal;
    private readonly IEnumerable<IWorkflowTypeEventHandler> _handlers;
    private readonly ILogger _logger;

    // The designer asks for the same workflow types for every task that runs one, and a background task resumes many
    // instances of the same types: they are loaded once per scope. A save or a delete clears them.
    private readonly Dictionary<string, WorkflowType> _byWorkflowTypeId = new(StringComparer.Ordinal);
    private IList<WorkflowType> _all;

    public WorkflowTypeStore(
        ISession session,
        IWorkflowTypeVersionStore versionStore,
        IWorkflowExecutionJournal journal,
        IEnumerable<IWorkflowTypeEventHandler> handlers,
        ILogger<WorkflowTypeStore> logger)
    {
        _session = session;
        _versionStore = versionStore;
        _journal = journal;
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

    public async Task<WorkflowType> GetAsync(string workflowTypeId)
    {
        if (string.IsNullOrEmpty(workflowTypeId))
        {
            return await _session.Query<WorkflowType, WorkflowTypeIndex>(x => x.WorkflowTypeId == workflowTypeId).FirstOrDefaultAsync();
        }

        if (!_byWorkflowTypeId.TryGetValue(workflowTypeId, out var workflowType))
        {
            workflowType = _all?.FirstOrDefault(x => x.WorkflowTypeId == workflowTypeId)
                ?? await _session.Query<WorkflowType, WorkflowTypeIndex>(x => x.WorkflowTypeId == workflowTypeId).FirstOrDefaultAsync();

            if (workflowType is not null)
            {
                _byWorkflowTypeId[workflowTypeId] = workflowType;
            }
        }

        return workflowType;
    }

    public async Task<IEnumerable<WorkflowType>> ListAsync()
    {
        return _all ??= (await _session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync()).ToList();
    }

    public async Task<IEnumerable<WorkflowType>> GetByStartActivityAsync(string activityName)
    {
        return await _session
            .Query<WorkflowType, WorkflowTypeStartActivitiesIndex>(index =>
                index.StartActivityName == activityName &&
                index.IsEnabled)
            .ListAsync();
    }

    public async Task SaveAsync(WorkflowType workflowType)
    {
        var isNew = workflowType.Id == 0;
        Clear();

        // Every change to what runs creates a version, so the workflow type is always its latest version.
        await _versionStore.CreateIfChangedAsync(workflowType);
        await _session.SaveAsync(workflowType);

        if (isNew)
        {
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
        Clear();

        // Delete workflows first.
        var workflows = await _session.Query<Workflow, WorkflowIndex>(x => x.WorkflowTypeId == workflowType.WorkflowTypeId).ListAsync();

        foreach (var workflow in workflows)
        {
            _session.Delete(workflow);
        }

        await _journal.DeleteAsync(workflows.Select(workflow => workflow.WorkflowId));

        // Then delete the workflow type and its versions.
        await _versionStore.DeleteAsync(workflowType.WorkflowTypeId);
        _session.Delete(workflowType);
        var context = new WorkflowTypeDeletedContext(workflowType);
        await _handlers.InvokeAsync((handler, context) => handler.DeletedAsync(context), context, _logger);
    }

    private void Clear()
    {
        _byWorkflowTypeId.Clear();
        _all = null;
    }
}
