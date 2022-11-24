using System;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.FileStorage.AzureBlob;
public class BlobFileStoreFactory
{
    private readonly IAzureClientFactory<BlobServiceClient> _clientFactory;
    private readonly IServiceProvider _serviceProvider;

    private const string DefaultStorageName = "Default";

    public BlobFileStoreFactory(IAzureClientFactory<BlobServiceClient> clientFactory, IServiceProvider serviceProvider)
    {
        _clientFactory = clientFactory;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a <see cref="BlobFileStore"/> from <see cref="BlobStorageOptions"/>
    /// If a connectionstring is specified, it will use the connectionstring
    /// otherwise it will try to get the BlobServiceClient configured with the specified name in the BlobServiceName property
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public virtual BlobFileStore Create(BlobStorageOptions options)
    {
        var containerClient = CreateContainerClientFromOptions(options);

        return new BlobFileStore(
            containerClient,
            options.BasePath,
            _serviceProvider.GetRequiredService<IClock>(),
            _serviceProvider.GetRequiredService<IContentTypeProvider>());
    }


    private BlobContainerClient CreateContainerClientFromOptions(BlobStorageOptions options)
    {
        // return a container client from connectionstring, if specified
        if (!String.IsNullOrEmpty(options.ConnectionString))
        {
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        }

        // else use the clientFactory to lookup a BlobService with the specified name
        if (!String.IsNullOrEmpty(options.BlobServiceName))
        {
            return _clientFactory
                .CreateClient(options.BlobServiceName)
                .GetBlobContainerClient(options.ContainerName);
        }

        // fall-back to the default registered blob storage
        return _clientFactory
            .CreateClient(DefaultStorageName)
            .GetBlobContainerClient(options.ContainerName);
    }

}
