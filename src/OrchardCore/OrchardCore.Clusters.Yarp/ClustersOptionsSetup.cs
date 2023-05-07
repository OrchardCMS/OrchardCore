using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OrchardCore.Clusters;

/// <summary>
/// Configures the <see cref="ClustersOptions"/> from the application configuration.
/// </summary>
public class ClustersOptionsSetup : IConfigureOptions<ClustersOptions>
{
    private readonly ClustersConfiguration _configuration;

    public ClustersOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration.GetSection("OrchardCore_Clusters").Get<ClustersConfiguration>();
    }

    public void Configure(ClustersOptions options)
    {
        // Check if there is at least one configured cluster.
        if (_configuration is null || _configuration.Clusters is null || _configuration.Clusters.Count == 0)
        {
            return;
        }

        // Configure global clusters options.
        options.Enabled = _configuration.Enabled;
        options.Hosts = _configuration.Hosts ?? Array.Empty<string>();

        // Configure all single cluster options.
        foreach (var cluster in _configuration.Clusters)
        {
            var slotRange = cluster.Value.SlotRange;
            if (slotRange is not null && slotRange.Length == 2)
            {
                // Set options per single ckuster.
                options.Clusters.Add(new ClusterOptions
                {
                    ClusterId = cluster.Key,
                    SlotMin = cluster.Value.SlotRange[0],
                    SlotMax = cluster.Value.SlotRange[1],
                    MaxIdleTime = cluster.Value.MaxIdleTime,
                });
            }
        }
    }
}
