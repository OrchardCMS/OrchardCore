using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Handlers;

/// <summary>
/// Deletes the draft of a workflow type when the type is deleted, including when the <c>WorkflowType</c>
/// recipe step replaces it.
/// </summary>
internal sealed class WorkflowTypeDraftHandler : WorkflowTypeHandlerBase
{
    // This handler is resolved by 'IWorkflowTypeStore', so it uses the session instead of
    // 'IWorkflowTypeDraftManager', which depends on the store.
    private readonly ISession _session;

    public WorkflowTypeDraftHandler(ISession session)
    {
        _session = session;
    }

    public override async Task DeletedAsync(WorkflowTypeDeletedContext context)
    {
        var drafts = await _session.Query<WorkflowTypeDraft, WorkflowTypeDraftIndex>(index => index.WorkflowTypeId == context.WorkflowType.WorkflowTypeId)
            .ListAsync();

        foreach (var draft in drafts)
        {
            _session.Delete(draft);
        }
    }
}
