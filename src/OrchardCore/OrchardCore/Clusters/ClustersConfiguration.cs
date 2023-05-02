using System.Collections.Generic;

namespace OrchardCore.Clusters;

public class ClustersConfiguration
{
    public bool UseAsProxy { get; init; }
    public string[] ProxyHosts { get; init; }
    public IReadOnlyDictionary<string, ClusterConfiguration> Clusters { get; init; }
}
