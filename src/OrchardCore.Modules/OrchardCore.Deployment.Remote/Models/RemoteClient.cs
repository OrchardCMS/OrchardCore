using System;

namespace OrchardCore.Deployment.Remote.Models
{
    public class RemoteClient
    {
        public string Id { get; set; }
        public string ClientName { get; set; }

        [Obsolete("Api keys now are persisted in a secret store, will be removed in a future version.")]
        public byte[] ProtectedApiKey { get; set; }
    }
}
