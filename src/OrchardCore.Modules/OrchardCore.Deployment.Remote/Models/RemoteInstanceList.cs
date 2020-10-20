using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Deployment.Remote.Models
{
    public class RemoteInstanceList : Document
    {
        public List<RemoteInstance> RemoteInstances { get; set; } = new List<RemoteInstance>();
    }
}
