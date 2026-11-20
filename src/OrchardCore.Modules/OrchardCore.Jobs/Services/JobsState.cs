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

    /// <summary>
    /// Gets or sets the time zone of the site the next occurrences of the schedules were last checked against, so
    /// they are only checked again when it changes.
    /// </summary>
    public string SiteTimeZoneId { get; set; }
}
