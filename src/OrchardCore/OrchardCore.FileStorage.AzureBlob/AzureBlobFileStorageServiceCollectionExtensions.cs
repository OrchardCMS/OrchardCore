using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.FileStorage.AzureBlob;

public static class AzureBlobFileStorageServiceCollectionExtensions
{
    /// <summary>
    /// Registers the BlobFileStorage services in the ServiceCollection.
    /// Note: this method can be called multiple times
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAzureBlobFileStorage(this IServiceCollection services)
    {
        // always use TryXXX methods because this method can be called multiple times
        // by different modules (currently: Azure Media and Azure Shells module)
        services.TryAddSingleton<BlobFileStoreFactory>();
        return services;
    }
}
