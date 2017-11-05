namespace OrchardCore.FileStorage.AzureBlob
{
    public abstract class BlobStorageOptions
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string BasePath { get; set; }
    }
}
