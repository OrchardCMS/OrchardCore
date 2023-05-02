using System;
using System.Collections.Generic;

namespace OrchardCore.Clusters;

public class ClustersOptions
{
    public bool Enabled { get; set; }
    public string[] Hosts { get; set; } = Array.Empty<string>();
    public List<ClusterOptions> Clusters { get; set; } = new();
}
