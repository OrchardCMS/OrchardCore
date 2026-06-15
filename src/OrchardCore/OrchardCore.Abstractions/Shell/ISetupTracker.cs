namespace OrchardCore.Environment.Shell;

/// <summary>
/// Tracks active tenant setup operations to prevent concurrent setups and
/// accidental deletion of tenants during setup. Uses a local dictionary for
/// fast in-process checks and a distributed cache for cross-instance coordination.
/// </summary>
public interface ISetupTracker
{
    /// <summary>
    /// Determines whether setup is currently in progress for the specified tenant.
    /// Checks the local process first and, in distributed environments, also checks
    /// the distributed cache for setup operations running on other instances.
    /// </summary>
    /// <param name="shellSettings">The shell settings of the tenant to check.</param>
    /// <returns><see langword="true"/> if setup is actively running; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsSetupInProgressAsync(ShellSettings shellSettings);

    /// <summary>
    /// Attempts to mark setup as started for the specified tenant. Uses a distributed lock
    /// briefly to perform an atomic check-and-write to the distributed cache, then releases
    /// the lock. Returns <see langword="false"/> if setup is already in progress on this or
    /// another instance.
    /// </summary>
    /// <param name="shellSettings">The shell settings of the tenant.</param>
    /// <returns><see langword="true"/> if the setup was successfully marked as started; <see langword="false"/> if setup is already running.</returns>
    Task<bool> TryMarkSetupStartedAsync(ShellSettings shellSettings);

    /// <summary>
    /// Marks setup as completed (success or failure) for the specified tenant,
    /// removing the entry from both the local tracker and the distributed cache.
    /// </summary>
    /// <param name="shellSettings">The shell settings of the tenant.</param>
    Task MarkSetupCompletedAsync(ShellSettings shellSettings);
}
