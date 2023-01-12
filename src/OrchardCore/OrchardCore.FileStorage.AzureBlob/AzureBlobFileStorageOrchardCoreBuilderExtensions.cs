using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.FileStorage.AzureBlob;
public static class AzureBlobFileStorageOrchardCoreBuilderExtensions
{
    /// <summary>
    /// This registers the AzureBlobFileStorage components.
    /// Note: this method is safe to call more then once.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static OrchardCoreBuilder AddAzureBlobFileStorage(this OrchardCoreBuilder builder)
    {
        builder.ApplicationServices.AddAzureBlobFileStorage();
        return builder;
    }
}
