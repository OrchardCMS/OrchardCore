using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;

namespace OrchardCore.FileStorage.AzureBlob;

public class BlobContainerClientFactory
{
    private readonly IAzureClientFactory<BlobServiceClient> _azureClientFactory;

    public BlobContainerClientFactory(IAzureClientFactory<BlobServiceClient> azureClientFactory)
    {
        _azureClientFactory = azureClientFactory;
    }

    public BlobContainerClient Create(BlobStorageOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.AzureClientName))
        {
            return _azureClientFactory.CreateClient(options.AzureClientName).GetBlobContainerClient(options.ContainerName);
        }
        else
        {
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        }
    }
}
