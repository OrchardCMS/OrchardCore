using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure.Services;

/// <summary>
/// A Gen2-oriented <see cref="IFileStore"/> wrapper for media that keeps Blob operations
/// while relying on Data Lake directory semantics where needed.
/// When HNS (Hierarchical Namespace) is detected, operations use native DataLake APIs for
/// atomic moves, real directories, and efficient listing. Otherwise, falls back to flat
/// Blob Storage behavior via <see cref="BlobFileStore"/>.
/// </summary>
public sealed class Gen2BlobFileStore : IFileStore
{
    private const string DirectoryMarkerFileName = "OrchardCore.Media.txt";

    private readonly BlobFileStore _blobFileStore;
    private readonly IClock _clock;
    private readonly BlobContainerClient _blobContainer;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly ILogger _logger;
    private readonly string _basePrefix;
    private readonly bool? _useHierarchicalNamespaceOverride;
    private readonly SemaphoreSlim _capabilitiesLock = new(1, 1);
    private IFileStoreCapabilities _capabilities;

    public Gen2BlobFileStore(MediaBlobStorageOptions options, IClock clock, IContentTypeProvider contentTypeProvider, ILogger logger)
    {
        _blobFileStore = new BlobFileStore(options, clock, contentTypeProvider);
        _clock = clock;
        _blobContainer = new BlobContainerClient(options.ConnectionString, options.ContainerName);
        _blobServiceClient = new BlobServiceClient(options.ConnectionString);
        _useHierarchicalNamespaceOverride = options.UseHierarchicalNamespace;
        _logger = logger;

        var serviceClient = new DataLakeServiceClient(options.ConnectionString);
        _dataLakeFileSystemClient = serviceClient.GetFileSystemClient(options.ContainerName);

        if (!string.IsNullOrEmpty(options.BasePath))
        {
            _basePrefix = NormalizePrefix(options.BasePath);
        }
    }

    public IFileStoreCapabilities Capabilities => _capabilities ?? FileStoreCapabilities.Default;

    /// <summary>
    /// Probes the storage account to determine whether Hierarchical Namespace (HNS) is enabled.
    /// Must be called once at startup; after completion <see cref="Capabilities"/> returns the detected values.
    /// </summary>
    public async Task EnsureCapabilitiesAsync()
    {
        if (_capabilities is not null)
        {
            return;
        }

        await _capabilitiesLock.WaitAsync();
        try
        {
            if (_capabilities is not null)
            {
                return;
            }

            var hnsEnabled = _useHierarchicalNamespaceOverride;

            if (!hnsEnabled.HasValue)
            {
                try
                {
                    var accountInfo = await _blobServiceClient.GetAccountInfoAsync();
                    hnsEnabled = accountInfo.Value.IsHierarchicalNamespaceEnabled;
                }
                catch (Exception ex)
                {
                    // If we cannot determine HNS status (e.g., insufficient permissions on SAS token),
                    // default to false (flat namespace behavior).
                    _logger.LogWarning(ex, "Unable to detect Azure Blob Storage Hierarchical Namespace status. Falling back to flat namespace behavior.");
                    hnsEnabled = false;
                }
            }

            _capabilities = new FileStoreCapabilities(
                hasHierarchicalNamespace: hnsEnabled.Value,
                supportsAtomicMove: hnsEnabled.Value);

            if (hnsEnabled.Value)
            {
                _logger.LogInformation("Azure Blob Storage Hierarchical Namespace (ADLS Gen2) detected. Using native directory and atomic move operations.");
            }
            else
            {
                _logger.LogInformation("Azure Blob Storage flat namespace detected. Using standard blob operations with virtual directories.");
            }
        }
        finally
        {
            _capabilitiesLock.Release();
        }
    }

    public Task CopyFileAsync(string srcPath, string dstPath)
        => _blobFileStore.CopyFileAsync(srcPath, dstPath);

    public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        => _blobFileStore.CreateFileFromStreamAsync(path, inputStream, overwrite);

    public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
    {
        if (Capabilities.HasHierarchicalNamespace)
        {
            try
            {
                if (path == string.Empty)
                {
                    return new BlobDirectory(path, _clock.UtcNow);
                }

                var prefix = this.Combine(_basePrefix, path);
                var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(prefix);

                if (await directoryClient.ExistsAsync())
                {
                    return new BlobDirectory(path, _clock.UtcNow);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get directory info with path '{path}'.", ex);
            }
        }

        return await _blobFileStore.GetDirectoryInfoAsync(path);
    }

    public IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
    {
        try
        {
            return includeSubDirectories
                ? GetDirectoryContentFlatAsync(path)
                : GetDirectoryContentByHierarchyAsync(path);
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot get directory content with path '{path}'.", ex);
        }
    }

    public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        => _blobFileStore.GetFileInfoAsync(path);

    public Task<Stream> GetFileStreamAsync(string path)
        => _blobFileStore.GetFileStreamAsync(path);

    public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        => _blobFileStore.GetFileStreamAsync(fileStoreEntry);

    public async Task MoveFileAsync(string oldPath, string newPath)
    {
        if (Capabilities.SupportsAtomicMove)
        {
            try
            {
                var oldFullPath = this.Combine(_basePrefix, oldPath);
                var newFullPath = this.Combine(_basePrefix, newPath);

                var fileClient = _dataLakeFileSystemClient.GetFileClient(oldFullPath);
                await fileClient.RenameAsync(newFullPath);
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot move file '{oldPath}' to '{newPath}'.", ex);
            }

            return;
        }

        await _blobFileStore.MoveFileAsync(oldPath, newPath);
    }

    public async Task<bool> TryCreateDirectoryAsync(string path)
    {
        if (Capabilities.HasHierarchicalNamespace)
        {
            try
            {
                var prefix = this.Combine(_basePrefix, path);
                var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(prefix);
                var response = await directoryClient.CreateIfNotExistsAsync();

                // CreateIfNotExistsAsync returns null if it already existed.
                return response is not null;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot create directory '{path}'.", ex);
            }
        }

        return await _blobFileStore.TryCreateDirectoryAsync(path);
    }

    public async Task<bool> TryDeleteDirectoryAsync(string path)
    {
        if (Capabilities.HasHierarchicalNamespace)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new FileStoreException("Cannot delete the root directory.");
                }

                var prefix = this.Combine(_basePrefix, path);
                var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(prefix);

                if (!await directoryClient.ExistsAsync())
                {
                    return false;
                }

                await directoryClient.DeleteAsync(recursive: true);
                return true;
            }
            catch (FileStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot delete directory '{path}'.", ex);
            }
        }

        return await _blobFileStore.TryDeleteDirectoryAsync(path);
    }

    public Task<bool> TryDeleteFileAsync(string path)
        => _blobFileStore.TryDeleteFileAsync(path);

    private async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentByHierarchyAsync(string path = null)
    {
        var prefix = this.Combine(_basePrefix, path);
        prefix = NormalizePrefix(prefix);

        var page = _blobContainer.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.None, "/", prefix, CancellationToken.None);

        await foreach (var blob in page)
        {
            if (blob.IsPrefix)
            {
                var folderPath = blob.Prefix;
                if (!string.IsNullOrEmpty(_basePrefix))
                {
                    folderPath = folderPath[(_basePrefix.Length - 1)..];
                }

                folderPath = folderPath.Trim('/');

                if (blob.Blob is not null && blob.Blob.Properties is not null)
                {
                    yield return new BlobDirectory(
                        folderPath,
                        blob.Blob.Properties.LastModified.HasValue
                            ? blob.Blob.Properties.LastModified.Value.DateTime
                            : _clock.UtcNow);
                }
                else
                {
                    yield return new BlobDirectory(folderPath, _clock.UtcNow);
                }

                continue;
            }

            var itemName = Path.GetFileName(blob.Blob.Name).Trim('/');

            if (!string.Equals(itemName, DirectoryMarkerFileName, StringComparison.Ordinal))
            {
                var itemPath = this.Combine(path?.Trim('/'), itemName);
                yield return new BlobFile(itemPath, blob.Blob.Properties.ContentLength, blob.Blob.Properties.LastModified);
            }
        }
    }

    private async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentFlatAsync(string path = null)
    {
        var prefix = this.Combine(_basePrefix, path);
        prefix = NormalizePrefix(prefix);

        var page = _blobContainer.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, prefix, CancellationToken.None);
        await foreach (var blob in page)
        {
            var name = blob.Name;

            if (blob.Metadata.TryGetValue("hdi_isfolder", out var value) &&
                value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var directoryName = name;
                if (!string.IsNullOrEmpty(_basePrefix))
                {
                    directoryName = directoryName[(_basePrefix.Length - 1)..];
                }

                if (blob.Properties is not null)
                {
                    yield return new BlobDirectory(
                        directoryName,
                        blob.Properties.LastModified.HasValue
                            ? blob.Properties.LastModified.Value.DateTime
                            : _clock.UtcNow);
                }
                else
                {
                    yield return new BlobDirectory(directoryName, _clock.UtcNow);
                }

                continue;
            }

            if (name.EndsWith(DirectoryMarkerFileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(_basePrefix))
            {
                name = name[(_basePrefix.Length - 1)..];
            }

            yield return new BlobFile(name, blob.Properties.ContentLength, blob.Properties.LastModified);
        }
    }

    private static string NormalizePrefix(string prefix)
    {
        prefix = prefix.Trim('/') + '/';
        if (prefix.Length == 1)
        {
            return string.Empty;
        }

        return prefix;
    }
}
