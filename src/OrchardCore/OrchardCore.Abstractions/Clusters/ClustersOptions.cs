using System;
using System.Collections.Generic;

namespace OrchardCore.Clusters;

public class ClustersOptions
{
    public bool UseAsProxy { get; set; }
    public string[] ProxyHosts { get; set; } = Array.Empty<string>();
    public List<ClusterOptions> Clusters { get; set; } = new();
}
