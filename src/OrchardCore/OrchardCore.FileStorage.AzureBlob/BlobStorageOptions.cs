using Azure.Core;

namespace OrchardCore.FileStorage.AzureBlob
{
    public abstract class BlobStorageOptions
    {
        /// <summary>
        /// The Azure Blob connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The reference to a named AzureClient.
        /// The client needs to be registered using the AddAzureClients extension method.
        /// more info: https://learn.microsoft.com/en-us/dotnet/azure/sdk/dependency-injection#configure-multiple-service-clients-with-different-names
        /// </summary>
        public string BlobServiceName { get; set; }

        /// <summary>
        /// The Azure Blob container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// The base directory path to use inside the container for this stores contents.
        /// </summary>
        public string BasePath { get; set; }
    }
}
