namespace OrchardCore.FileStorage.AzureBlob
{
    public abstract class BlobStorageOptions
    {
        /// <summary>
        /// The Azure Blob connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The Azure Blob container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// The base directory path to use inside the container for this stores contents.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// The Azure Client name. Must be configured by AddAzureClients on Startup.
        /// https://learn.microsoft.com/en-us/dotnet/azure/sdk/dependency-injection?tabs=web-app-builder#configure-multiple-service-clients-with-different-names
        /// </summary>
        public string AzureClientName { get; set; }
    }
}
