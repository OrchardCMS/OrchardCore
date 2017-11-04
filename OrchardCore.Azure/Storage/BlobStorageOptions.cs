namespace OrchardCore.Azure.Storage
{
    public class BlobStorageOptions
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string BasePath { get; set; }
    }
}
