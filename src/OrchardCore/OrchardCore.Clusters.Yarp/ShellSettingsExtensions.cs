using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Extension methods for managing tenant clusters.
/// </summary>
public static class ShellSettingsExtensions
{
    /// <summary>
    /// Returns the tenant cluster slot computed from the <see cref="ShellSettings.TenantId"/>,
    /// or -1 if the tenant doesn't have an identifier yet.
    /// </summary>
    public static int GetClusterSlot(this ShellSettings settings) =>
        settings.TenantId is null ? -1 : Crc16XModem.Compute(settings.TenantId) % ClustersOptions.SlotsCount;

    /// <summary>
    /// Returns the selected tenant cluster based on the provided <see cref="ShellSettings"/>.
    /// </summary>
    public static string GetClusterId(this ShellSettings settings, ClustersOptions options)
    {
        var slot = settings.GetClusterSlot();

        foreach (var cluster in options.Clusters)
        {
            // Check if the cluster slot of the current tenant is in the cluster slot range.
            if (slot >= cluster.SlotMin && slot <= cluster.SlotMax)
            {
                return cluster.ClusterId;
            }
        }

        return null;
    }
}
