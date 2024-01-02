using System;

namespace OrchardCore.Deployment.Remote.Models
{
    public class RemoteInstance
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ClientName { get; set; }
        public string Url { get; set; }

        [Obsolete("Api keys now are persisted in a secret store, will be removed in a future version.")]
        public string ApiKey { get; set; }
    }
}
