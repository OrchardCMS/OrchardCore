namespace OrchardCore.Jobs.Models;

/// <summary>
/// The names of what starts a run, stored in <see cref="JobRun.Trigger"/>. A module can use a name of its own.
/// </summary>
public static class JobTriggers
{
    /// <summary>
    /// The run was queued by the code of a module.
    /// </summary>
    public const string Code = "Code";

    /// <summary>
    /// The run was started by a user from the admin.
    /// </summary>
    public const string Manual = "Manual";

    /// <summary>
    /// The run was created by a schedule.
    /// </summary>
    public const string Schedule = "Schedule";

    /// <summary>
    /// The run was queued by another run, as one of its child runs.
    /// </summary>
    public const string Parent = "Parent";

    /// <summary>
    /// The run was queued by a workflow.
    /// </summary>
    public const string Workflow = "Workflow";
}
