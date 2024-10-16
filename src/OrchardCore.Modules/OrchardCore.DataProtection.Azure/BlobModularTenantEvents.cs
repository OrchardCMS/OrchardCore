using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.DataProtection.Azure;

/// <summary>
/// A tenant event that deletes data protection blobs from a container when a tenant is deleted.
/// </summary>
internal sealed class BlobModularTenantEvents : ModularTenantEvents
{
    private readonly BlobOptions _blobOptions;
    private readonly ILogger _logger;

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
    public override Task RemovingAsync(ShellRemovingContext context)
    {
        var blobClient = new BlobClient(
            _blobOptions.ConnectionString,
            _blobOptions.ContainerName,
            _blobOptions.BlobName);

        _logger.LogDebug("Deleting blob '{BlobName}' from container '{ContainerName}'.", _blobOptions.BlobName, _blobOptions.ContainerName);

        return blobClient.DeleteIfExistsAsync();
    }
}
