namespace OrchardCore.Workflows.Services;

/// <summary>
/// Options of the workflow type versions, bound to the <c>OrchardCore:Workflows:Versions</c> configuration section.
/// </summary>
public sealed class WorkflowVersionOptions
{
    /// <summary>
    /// The number of most recent versions to keep per workflow type. When a new version is created, the older
    /// versions are deleted, except those that workflow instances run on. 0 (the default) keeps every version.
    /// </summary>
    public int MaxCount { get; set; }
}
