namespace OrchardCore.Deployment.Remote.Models
{
    public class RemoteClient
    {
        public string Id { get; set; }
        public string ClientName { get; set; }
        public byte[] ProtectedApiKey { get; set; }
    }
}
