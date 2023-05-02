using System.Collections.Generic;

namespace OrchardCore.Clusters;

public class ClustersConfiguration
{
    public bool Enabled { get; init; }
    public string[] Hosts { get; init; }
    public IReadOnlyDictionary<string, ClusterConfiguration> Clusters { get; init; }
}
