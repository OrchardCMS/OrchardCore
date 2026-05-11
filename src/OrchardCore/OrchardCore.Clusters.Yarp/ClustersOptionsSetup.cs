using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Configures the <see cref="ClustersOptions"/> from the application configuration.
/// </summary>
public class ClustersOptionsSetup : IConfigureOptions<ClustersOptions>
{
    private readonly IConfigurationSection _configurationSection;

    public ClustersOptionsSetup(IConfiguration configuration) => _configurationSection = configuration.GetSection("OrchardCore_Clusters");

    public void Configure(ClustersOptions options)
    {
        var configuration = _configurationSection.Get<ClustersConfiguration>();

        if (configuration is null)
        {
            return;
        }

        // Configure global clusters options.
        options.Enabled = configuration.Enabled;
        options.MaxIdleTime = configuration.MaxIdleTime;

        // Check if there is at least one configured cluster.
        if (configuration.Clusters is null || configuration.Clusters.Count == 0)
        {
            if (configuration.Enabled)
            {
                throw new InvalidOperationException("Tenant clustering is enabled but no clusters are configured.");
            }

            return;
        }

        // Configure all single cluster options.
        foreach (var cluster in configuration.Clusters)
        {
            var slotRange = cluster.Value?.SlotRange;

            if (slotRange is null || slotRange.Length != 2)
            {
                if (configuration.Enabled)
                {
                    throw new InvalidOperationException($"The tenant cluster '{cluster.Key}' must define a SlotRange with exactly two values.");
                }

                continue;
            }

            var clusterOptions = new ClusterOptions
            {
                ClusterId = cluster.Key,
                SlotMin = slotRange[0],
                SlotMax = slotRange[1],
            };

            if (configuration.Enabled)
            {
                ValidateSlotRange(clusterOptions);
                ValidateSlotRangeOverlaps(clusterOptions, options);
            }

            options.Clusters.Add(clusterOptions);
        }
    }

    private static void ValidateSlotRange(ClusterOptions cluster)
    {
        if (cluster.SlotMin < 0 || cluster.SlotMax >= ShellSettings.ClusterSlotsCount)
        {
            throw new InvalidOperationException($"The tenant cluster '{cluster.ClusterId}' SlotRange must be within 0 and {ShellSettings.ClusterSlotsCount - 1}.");
        }

        if (cluster.SlotMin > cluster.SlotMax)
        {
            throw new InvalidOperationException($"The tenant cluster '{cluster.ClusterId}' SlotRange minimum must be less than or equal to the maximum.");
        }
    }

    private static void ValidateSlotRangeOverlaps(ClusterOptions cluster, ClustersOptions options)
    {
        foreach (var existingCluster in options.Clusters)
        {
            if (cluster.SlotMin <= existingCluster.SlotMax && existingCluster.SlotMin <= cluster.SlotMax)
            {
                throw new InvalidOperationException($"The tenant cluster '{cluster.ClusterId}' SlotRange overlaps with the tenant cluster '{existingCluster.ClusterId}' SlotRange.");
            }
        }
    }
}
