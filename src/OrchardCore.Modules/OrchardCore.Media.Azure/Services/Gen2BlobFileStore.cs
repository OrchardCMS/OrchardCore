using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake;
using Microsoft.AspNetCore.StaticFiles;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure.Services;

/// <summary>
/// A Gen2-oriented <see cref="IFileStore"/> wrapper for media that keeps Blob operations
/// while relying on Data Lake directory semantics where needed.
/// </summary>
public sealed class Gen2BlobFileStore : IFileStore
{
    private const string DirectoryMarkerFileName = "OrchardCore.Media.txt";

    private readonly BlobFileStore _blobFileStore;
    private readonly IClock _clock;
    private readonly BlobContainerClient _blobContainer;
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly string _basePrefix;

    public Gen2BlobFileStore(BlobStorageOptions options, IClock clock, IContentTypeProvider contentTypeProvider)
    {
        _blobFileStore = new BlobFileStore(options, clock, contentTypeProvider);
        _clock = clock;
        _blobContainer = new BlobContainerClient(options.ConnectionString, options.ContainerName);

        var serviceClient = new DataLakeServiceClient(options.ConnectionString);
        _dataLakeFileSystemClient = serviceClient.GetFileSystemClient(options.ContainerName);

        if (!string.IsNullOrEmpty(options.BasePath))
        {
            _basePrefix = NormalizePrefix(options.BasePath);
        }
    }

    public Task CopyFileAsync(string srcPath, string dstPath)
        => _blobFileStore.CopyFileAsync(srcPath, dstPath);

    public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        => _blobFileStore.CreateFileFromStreamAsync(path, inputStream, overwrite);

    public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        => _blobFileStore.GetDirectoryInfoAsync(path);

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

    public Task MoveFileAsync(string oldPath, string newPath)
        => _blobFileStore.MoveFileAsync(oldPath, newPath);

    public Task<bool> TryCreateDirectoryAsync(string path)
        => _blobFileStore.TryCreateDirectoryAsync(path);

    public async Task<bool> TryDeleteDirectoryAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new FileStoreException("Cannot delete the root directory.");
            }

            var prefix = _blobFileStore.Combine(_basePrefix, path);
            prefix = NormalizePrefix(prefix);

            var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(prefix);
            if (!await directoryClient.ExistsAsync())
            {
                throw new FileStoreException($"{prefix} does not exist.");
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
