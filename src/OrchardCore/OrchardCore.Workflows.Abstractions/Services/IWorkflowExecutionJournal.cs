using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// The journal of the activities that workflow instances executed.
/// </summary>
public interface IWorkflowExecutionJournal
{
    /// <summary>
    /// Whether executions are recorded (<c>OrchardCore:Workflows:Journal:Enabled</c>).
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Saves records of an instance, then deletes its oldest records beyond the configured maximum.
    /// </summary>
    /// <param name="workflowId">The <see cref="Workflow.WorkflowId"/>.</param>
    /// <param name="records">The records, with their sequence numbers.</param>
    Task SaveAsync(string workflowId, IEnumerable<WorkflowExecutionRecord> records);

    /// <summary>
    /// Returns the most recent records of an instance, in the order they were recorded.
    /// </summary>
    /// <param name="workflowId">The <see cref="Workflow.WorkflowId"/>.</param>
    /// <param name="count">The maximum number of records.</param>
    Task<IReadOnlyList<WorkflowExecutionRecord>> ListAsync(string workflowId, int count = 500);

    /// <summary>
    /// Returns a record of an instance, or <see langword="null"/> when it doesn't have one with this sequence number.
    /// </summary>
    /// <param name="workflowId">The <see cref="Workflow.WorkflowId"/>.</param>
    /// <param name="sequence">The <see cref="WorkflowExecutionRecord.Sequence"/>.</param>
    Task<WorkflowExecutionRecord> GetAsync(string workflowId, int sequence);

    /// <summary>
    /// Deletes the records of instances.
    /// </summary>
    /// <param name="workflowIds">The <see cref="Workflow.WorkflowId"/> of each instance.</param>
    Task DeleteAsync(IEnumerable<string> workflowIds);
}
