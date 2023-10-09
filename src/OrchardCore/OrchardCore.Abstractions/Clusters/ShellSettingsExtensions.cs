using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Extension methods for managing tenant clusters.
/// </summary>
public static class ShellSettingsExtensions
{
    /// <summary>
    /// Returns the selected tenant cluster based on the provided <see cref="ShellSettings"/>.
    /// </summary>
    public static string GetClusterId(this ShellSettings settings, ClustersOptions options)
    {
        foreach (var cluster in options.Clusters)
        {
            // Check if the cluster slot of the current tenant is in the cluster slot range.
            if (settings.ClusterSlot >= cluster.SlotMin && settings.ClusterSlot <= cluster.SlotMax)
            {
                return cluster.ClusterId;
            }
        }

        return null;
    }
}
