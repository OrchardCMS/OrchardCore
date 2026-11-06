using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Helpers;

internal static class WorkflowFaultExtensions
{
    /// <summary>
    /// The activity a faulted instance faulted at: the one its retry policy retries, or the last one its journal
    /// records as faulted.
    /// </summary>
    public static string GetFaultedActivityId(this Workflow workflow, IEnumerable<WorkflowExecutionRecord> journal)
        => workflow.PendingRetry?.ActivityId ?? LastFaultedActivityId(journal);

    /// <summary>
    /// Like <see cref="GetFaultedActivityId"/>, loading the journal only when the instance has no pending retry.
    /// </summary>
    public static async Task<string> GetFaultedActivityIdAsync(this IWorkflowExecutionJournal journal, Workflow workflow)
        => workflow.PendingRetry?.ActivityId ?? LastFaultedActivityId(await journal.ListAsync(workflow.WorkflowId));

    private static string LastFaultedActivityId(IEnumerable<WorkflowExecutionRecord> journal)
        => journal.LastOrDefault(record => record.Status == WorkflowExecutionRecordStatus.Faulted)?.ActivityId;
}
