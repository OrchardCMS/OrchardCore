namespace OrchardCore.Jobs.Services;

/// <summary>
/// What this server already did for the tenant since it started. Registered as a tenant singleton.
/// </summary>
public sealed class JobsState
{
    /// <summary>
    /// Gets or sets a value indicating whether the schedules the jobs declare were created.
    /// </summary>
    public bool DeclaredSchedulesEnsured { get; set; }
}
