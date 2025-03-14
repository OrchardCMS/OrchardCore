namespace OrchardCore.Workflows.Models;

// When adding a new value, also add a corresponding LocalizedHtmlString to the dictionary in the trimming settings
// editor (WorkflowTrimming.Fields.Edit.cshtml).
public enum WorkflowStatus
{
    Idle,
    Starting,
    Resuming,
    Executing,
    Halted,
    Finished,
    Faulted,
    Aborted
}
