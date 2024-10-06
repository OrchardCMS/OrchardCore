using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.DataProtection.Azure;

/// <summary>
/// Represents tenant event that deletes data protection blob from a container when a tenant is deleted.
/// </summary>
public class BlobModularTenantEvents : ModularTenantEvents
{
    private readonly BlobOptions _blobOptions;
    private readonly ILogger<BlobModularTenantEvents> _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="BlobModularTenantEvents"/>.
    /// </summary>
    /// <param name="blobOptions">The <see cref="BlobOptions"/></param>
    /// <param name="logger">The <see cref="ILogger"/></param>
    public BlobModularTenantEvents(
        BlobOptions blobOptions,
        ILogger<BlobModularTenantEvents> logger)
    {
        _blobOptions = blobOptions;
        _logger = logger;
    }

    /// <summary>
    /// Removes the data protection blob from the container when a tenant is deleted.
    /// </summary>
    /// <param name="context">The <see cref="ShellRemovingContext"/></param>
    /// <returns></returns>
    public async override Task RemovingAsync(ShellRemovingContext context)
    {
        var blobClient = new BlobClient(
            _blobOptions.ConnectionString,
            _blobOptions.ContainerName,
            _blobOptions.BlobName);

        _logger.LogDebug("Deleting blob '{BlobName}' from container '{ContainerName}'.", _blobOptions.BlobName, _blobOptions.ContainerName);

        await blobClient.DeleteIfExistsAsync();
    }
}
