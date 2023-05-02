using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OrchardCore.Clusters;

public class ClustersOptionsSetup : IConfigureOptions<ClustersOptions>
{
    private readonly ClustersConfiguration _configuration;

    public ClustersOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration.GetSection("OrchardCore_Clusters").Get<ClustersConfiguration>();
    }

    public void Configure(ClustersOptions options)
    {
        if (_configuration is null || _configuration.Clusters is null || _configuration.Clusters.Count == 0)
        {
            return;
        }

        options.UseAsProxy = _configuration.UseAsProxy;
        options.ProxyHosts = _configuration.ProxyHosts ?? Array.Empty<string>();

        foreach (var cluster in _configuration.Clusters)
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
