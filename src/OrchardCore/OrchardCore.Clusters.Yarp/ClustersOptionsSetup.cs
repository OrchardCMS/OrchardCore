using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

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

        // Check if there is at least one configured cluster.
        if (configuration is null || configuration.Clusters is null || configuration.Clusters.Count == 0)
        {
            return;
        }

        // Configure global clusters options.
        options.Enabled = configuration.Enabled;
        options.MaxIdleTime = configuration.MaxIdleTime;

        // Configure all single cluster options.
        foreach (var cluster in configuration.Clusters)
        {
            var slotRange = cluster.Value.SlotRange;
            if (slotRange is not null && slotRange.Length == 2)
            {
                options.Clusters.Add(new ClusterOptions
                {
                    ClusterId = cluster.Key,
                    SlotMin = cluster.Value.SlotRange[0],
                    SlotMax = cluster.Value.SlotRange[1],
                });
            }
        }
    }
}
