using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Clusters;

/// <summary>
/// Configures the <see cref="ClustersOptions"/> from the application configuration.
/// </summary>
public sealed class ClustersOptionsSetup : IConfigureOptions<ClustersOptions>
{
    private readonly IConfigurationSection _configurationSection;
    private readonly ILogger _logger;
    private ClustersOptions _lastValidOptions;

    public ClustersOptionsSetup(IConfiguration configuration, ILogger<ClustersOptionsSetup> logger)
    {
        _configurationSection = configuration.GetSection(ClustersOptions.ConfigurationSection);
        _logger = logger;
    }

    public void Configure(ClustersOptions options)
    {
        ClustersOptions configuredOptions;

        try
        {
            configuredOptions = BindOptions();
        }
        catch (InvalidOperationException ex) when (_lastValidOptions is not null)
        {
            // A configuration reload should not stop all the tenants, YARP also keeps its last valid configuration.
            _logger.LogError(ex, "The reloaded tenant clusters configuration is invalid, the last valid configuration is still used.");

            configuredOptions = _lastValidOptions;
        }

        _lastValidOptions = configuredOptions;

        options.Enabled = configuredOptions.Enabled;
        options.ProxySecret = configuredOptions.ProxySecret;
        options.MaxIdleTime = configuredOptions.MaxIdleTime;
        options.Clusters = configuredOptions.Clusters;
    }

    private ClustersOptions BindOptions()
    {
        var options = new ClustersOptions();
        var configuration = _configurationSection.Get<ClustersConfiguration>();

        if (configuration is null)
        {
            return options;
        }

        // Configure global clusters options.
        options.Enabled = configuration.Enabled;
        options.ProxySecret = configuration.ProxySecret;
        options.MaxIdleTime = configuration.MaxIdleTime;

        // The clusters are only used by an instance forwarding the requests.
        if (!configuration.Enabled)
        {
            return options;
        }

        // Check if there is at least one configured cluster.
        if (configuration.Clusters is null || configuration.Clusters.Count == 0)
        {
            throw new InvalidOperationException("Tenant clustering is enabled but no clusters are configured.");
        }

        // Configure all single cluster options.
        foreach (var cluster in configuration.Clusters)
        {
            var slotRange = cluster.Value?.SlotRange;

            if (slotRange is null || slotRange.Length != 2)
            {
                throw new InvalidOperationException($"The tenant cluster '{cluster.Key}' must define a SlotRange with exactly two values.");
            }

            var clusterOptions = new ClusterOptions
            {
                ClusterId = cluster.Key,
                SlotMin = slotRange[0],
                SlotMax = slotRange[1],
            };

            ValidateSlotRange(clusterOptions);
            ValidateSlotRangeOverlaps(clusterOptions, options);

            options.Clusters.Add(clusterOptions);
        }

        // Without this route nothing is forwarded, and all the requests would end up with an empty 404.
        if (!_configurationSection.GetSection($"Routes:{ClustersOptions.RouteTemplate}").Exists())
        {
            throw new InvalidOperationException($"Tenant clustering is enabled but the '{ClustersOptions.RouteTemplate}' route is not configured.");
        }

        // Without this secret a client could prevent a request from being forwarded, and run any tenant on this instance.
        if (string.IsNullOrWhiteSpace(configuration.ProxySecret))
        {
            throw new InvalidOperationException("Tenant clustering is enabled but the ProxySecret is not configured.");
        }

        return options;
    }

    private static void ValidateSlotRange(ClusterOptions cluster)
    {
        if (cluster.SlotMin < 0 || cluster.SlotMax >= ClustersOptions.SlotsCount)
        {
            throw new InvalidOperationException($"The tenant cluster '{cluster.ClusterId}' SlotRange must be within 0 and {ClustersOptions.SlotsCount - 1}.");
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
