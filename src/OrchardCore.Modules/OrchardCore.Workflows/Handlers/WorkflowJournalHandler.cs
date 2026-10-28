using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Handlers;

/// <summary>
/// Deletes the journal of a deleted workflow instance.
/// </summary>
public sealed class WorkflowJournalHandler : WorkflowHandlerBase
{
    private readonly IWorkflowExecutionJournal _journal;

    public WorkflowJournalHandler(IWorkflowExecutionJournal journal)
    {
        _journal = journal;
    }

    public override Task DeletedAsync(WorkflowDeletedContext context)
        => _journal.DeleteAsync([context.Workflow.WorkflowId]);
}
