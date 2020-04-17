using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Deployment.Remote.Models
{
    public class RemoteInstanceList : Document
    {
        public List<RemoteInstance> RemoteInstances { get; set; } = new List<RemoteInstance>();
        public Dictionary<string, RemoteInstance> RemoteInstances { get; set; } = new Dictionary<string, RemoteInstance>(StringComparer.OrdinalIgnoreCase);
    }
}
