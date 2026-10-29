namespace OrchardCore.Workflows.Services;

/// <summary>
/// The settings of the workflow journal (<c>OrchardCore:Workflows:Journal</c>).
/// </summary>
public sealed class WorkflowJournalOptions
{
    /// <summary>
    /// Whether the activities that instances execute are recorded. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The number of most recent records kept per instance; 0 keeps every record. Defaults to 1000.
    /// </summary>
    public int MaxRecordsPerInstance { get; set; } = 1000;
}
