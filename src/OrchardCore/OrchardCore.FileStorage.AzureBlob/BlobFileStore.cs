using System.Net;
using System.Net.Mime;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.FileStorage.AzureBlob;

/// <summary>
/// Provides an <see cref="IFileStore"/> implementation that targets an underlying Azure Blob Storage account.
/// </summary>
/// <remarks>
/// This store supports both flat-namespace (Gen1) and hierarchical-namespace / ADLS Gen2 storage accounts.
/// When HNS is detected (or forced via configuration), operations use native DataLake APIs for
/// atomic moves, real directories, and efficient listing. Otherwise, standard blob operations are used
/// with virtual directory semantics.
///
/// Directories have no physical manifestation in flat blob storage; we can obtain a reference to them, but
/// that reference can be created regardless of whether the directory exists, and it can only be used
/// as a scoping container to operate on blobs within that directory namespace.
///
/// As a consequence, in flat-namespace mode this provider generally behaves as if any given directory always
/// exists. To simulate "creating" a directory this provider creates a marker file inside the directory,
/// which makes the directory "exist" and appear when listing contents subsequently. This marker file is
/// ignored (excluded) when listing directory contents.
///
/// Note that the Blob Container is not created automatically, and existence of the Container is not verified.
///
/// Create the Blob Container before enabling a Blob File Store.
///
/// Azure Blob Storage will create the BasePath inside the container during the upload of the first file.
/// </remarks>
public class BlobFileStore : IFileStore
{
    private static readonly byte[] MarkerFileContent = "This is a directory marker file used by Orchard Core."u8.ToArray();
    private const string DirectoryMarkerFileName = "OrchardCore.Media.txt";

    private readonly BlobStorageOptions _options;
    private readonly IClock _clock;
    private readonly BlobContainerClient _blobContainer;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly ILogger _logger;
    private readonly bool? _useHierarchicalNamespaceOverride;
    private readonly SemaphoreSlim _capabilitiesLock = new(1, 1);
    private bool? _hnsEnabled;

    private readonly string _basePrefix;

    public BlobFileStore(
        BlobStorageOptions options,
        IClock clock,
        IContentTypeProvider contentTypeProvider,
        ILogger logger = null)
    {
        _options = options;
        _clock = clock;
        _contentTypeProvider = contentTypeProvider;
        _logger = logger;
        _blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
        _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
        _useHierarchicalNamespaceOverride = options.UseHierarchicalNamespace;

        if (!string.IsNullOrEmpty(_options.DfsEndpoint))
        {
            // Use explicit DFS endpoint (required for local emulators like Azurite
            // where the DFS endpoint runs on a separate port).
            var credential = ParseCredentialsFromConnectionString(_options.ConnectionString);
            var serviceClient = new DataLakeServiceClient(new Uri(_options.DfsEndpoint), credential);
            _dataLakeFileSystemClient = serviceClient.GetFileSystemClient(_options.ContainerName);
        }
        else
        {
            var serviceClient = new DataLakeServiceClient(_options.ConnectionString);
            _dataLakeFileSystemClient = serviceClient.GetFileSystemClient(_options.ContainerName);
        }

        if (!string.IsNullOrEmpty(_options.BasePath))
        {
            _basePrefix = NormalizePrefix(_options.BasePath);
        }
    }

    /// <summary>
    /// Probes the storage account to determine whether Hierarchical Namespace (HNS) is enabled.
    /// Must be called once at startup.
    /// </summary>
    public async Task EnsureCapabilitiesAsync()
    {
        if (_hnsEnabled.HasValue)
        {
            return;
        }

        await _capabilitiesLock.WaitAsync();
        try
        {
            if (_hnsEnabled.HasValue)
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
                    _logger?.LogWarning(ex, "Unable to detect Azure Blob Storage Hierarchical Namespace status. Falling back to flat namespace behavior.");
                    hnsEnabled = false;
                }
            }

            _hnsEnabled = hnsEnabled.Value;

            if (_hnsEnabled.Value)
            {
                _logger?.LogInformation("Azure Blob Storage Hierarchical Namespace (ADLS Gen2) detected. Using native directory and atomic move operations.");
            }
            else
            {
                _logger?.LogInformation("Azure Blob Storage flat namespace detected. Using standard blob operations with virtual directories.");
            }
        }
        finally
        {
            _capabilitiesLock.Release();
        }
    }

    public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
    {
        try
        {
            var blob = GetBlobReference(path);

            var properties = await blob.GetPropertiesAsync();

            return new BlobFile(path, properties.Value.ContentLength, properties.Value.LastModified);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            // Instead of ExistsAsync() check which is 'slow' if we're expecting to find the blob we rely on the exception.
            return null;
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot get file info with path '{path}'.", ex);
        }
    }

    public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
    {
        if (_hnsEnabled == true)
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
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot get directory info with path '{path}'.", ex);
            }
        }

        try
        {
            if (path == string.Empty)
            {
                return new BlobDirectory(path, _clock.UtcNow);
            }

            var blobDirectory = await GetBlobDirectoryReference(path);

            if (blobDirectory != null)
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

    public IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
    {
        try
        {
            if (includeSubDirectories)
            {
                return GetDirectoryContentFlatAsync(path);
            }
            else
            {
                return GetDirectoryContentByHierarchyAsync(path);
            }
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot get directory content with path '{path}'.", ex);
        }
    }

    private async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentByHierarchyAsync(string path = null)
    {
        path = this.NormalizePath(path);

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

            var itemName = Path.GetFileName(WebUtility.UrlDecode(blob.Blob.Name)).Trim('/');

            // Ignore directory marker files.
            if (!string.Equals(itemName, DirectoryMarkerFileName, StringComparison.Ordinal))
            {
                var itemPath = this.Combine(path?.Trim('/'), itemName);
                yield return new BlobFile(itemPath, blob.Blob.Properties.ContentLength, blob.Blob.Properties.LastModified);
            }
        }
    }

    private async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentFlatAsync(string path = null)
    {
        path = this.NormalizePath(path);

        var prefix = this.Combine(_basePrefix, path);
        prefix = NormalizePrefix(prefix);

        if (_hnsEnabled == true)
        {
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

            yield break;
        }

        // Flat namespace: infer directory hierarchy from blob paths.
        var directories = new HashSet<string>();

        var flatPage = _blobContainer.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, prefix, CancellationToken.None);
        await foreach (var blob in flatPage)
        {
            var name = WebUtility.UrlDecode(blob.Name);

            // A flat blob listing does not return a folder hierarchy.
            // We can infer a hierarchy by examining the paths returned for the file contents
            // and evaluate whether a directory exists and should be added to the results listing.
            var directory = Path.GetDirectoryName(name);

            // Strip base folder from directory name.
            if (!string.IsNullOrEmpty(_basePrefix))
            {
                directory = directory[(_basePrefix.Length - 1)..];
            }

            // Do not include root folder, or current path, or multiple folders in folder listing.
            if (!string.IsNullOrEmpty(directory) &&
                !directories.Contains(directory) &&
                (string.IsNullOrEmpty(path) ||
                !directory.EndsWith(path)))
            {
                directories.Add(directory);
                yield return new BlobDirectory(directory, _clock.UtcNow);
            }

            // Ignore directory marker files.
            if (!name.EndsWith(DirectoryMarkerFileName))
            {
                if (!string.IsNullOrEmpty(_basePrefix))
                {
                    name = name[(_basePrefix.Length - 1)..];
                }
                yield return new BlobFile(name.Trim('/'), blob.Properties.ContentLength, blob.Properties.LastModified);
            }
        }
    }

    public async Task<bool> TryCreateDirectoryAsync(string path)
    {
        if (_hnsEnabled == true)
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

        // Since directories are only created implicitly when creating blobs, we
        // simply pretend like we created the directory, unless there is already
        // a blob with the same path.
        try
        {
            var blobFile = GetBlobReference(path);

            if (await blobFile.ExistsAsync())
            {
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");
            }

            var blobDirectory = await GetBlobDirectoryReference(path);
            if (blobDirectory == null)
            {
                await CreateDirectoryAsync(path);
            }

            return true;
        }
        catch (FileStoreException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot create directory '{path}'.", ex);
        }
    }

    public async Task<bool> TryDeleteFileAsync(string path)
    {
        try
        {
            var blob = GetBlobReference(path);

            return await blob.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot delete file '{path}'.", ex);
        }
    }

    public async Task<bool> TryDeleteDirectoryAsync(string path)
    {
        if (_hnsEnabled == true)
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
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new FileStoreException($"Cannot delete directory '{path}'.", ex);
            }
        }

        try
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new FileStoreException("Cannot delete the root directory.");
            }

            var blobsWereDeleted = false;
            var prefix = this.Combine(_basePrefix, path);
            prefix = NormalizePrefix(prefix);

            var page = _blobContainer.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, prefix, CancellationToken.None);
            await foreach (var blob in page)
            {
                var blobReference = _blobContainer.GetBlobClient(blob.Name);
                await blobReference.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                blobsWereDeleted = true;
            }

            return blobsWereDeleted;
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

    public async Task MoveFileAsync(string oldPath, string newPath)
    {
        if (_hnsEnabled == true)
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

        try
        {
            await CopyFileAsync(oldPath, newPath);
            await TryDeleteFileAsync(oldPath);
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot move file '{oldPath}' to '{newPath}'.", ex);
        }
    }

    public async Task CopyFileAsync(string srcPath, string dstPath)
    {
        try
        {
            if (srcPath == dstPath)
            {
                throw new ArgumentException($"The values for {nameof(srcPath)} and {nameof(dstPath)} must not be the same.");
            }

            var oldBlob = GetBlobReference(srcPath);
            var newBlob = GetBlobReference(dstPath);

            if (!await oldBlob.ExistsAsync())
            {
                throw new FileStoreException($"Cannot copy file '{srcPath}' because it does not exist.");
            }

            if (await newBlob.ExistsAsync())
            {
                throw new FileStoreException($"Cannot copy file '{srcPath}' because a file already exists in the new path '{dstPath}'.");
            }

            await newBlob.StartCopyFromUriAsync(oldBlob.Uri);

            await Task.Delay(250);
            var properties = await newBlob.GetPropertiesAsync();

            while (properties.Value.CopyStatus == CopyStatus.Pending)
            {
                await Task.Delay(250);

                // Need to fetch properties or CopyStatus will never update.
                properties = await newBlob.GetPropertiesAsync();
            }

            if (properties.Value.CopyStatus != CopyStatus.Success)
            {
                throw new FileStoreException($"Error while copying file '{srcPath}'; copy operation failed with status {properties.Value.CopyStatus} and description {properties.Value.CopyStatusDescription}.");
            }
        }
        catch (FileStoreException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot copy file '{srcPath}' to '{dstPath}'.", ex);
        }
    }

    public async Task<Stream> GetFileStreamAsync(string path)
    {
        try
        {
            var blob = GetBlobReference(path);

            if (!await blob.ExistsAsync())
            {
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");
            }

            return (await blob.DownloadAsync()).Value.Content;
        }
        catch (FileStoreException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot get file stream of the file '{path}'.", ex);
        }
    }

    // Reduces the need to call blob.FetchAttributes, and blob.ExistsAsync,
    // as Azure Storage Library will perform these actions on OpenReadAsync().
    public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
    {
        return GetFileStreamAsync(fileStoreEntry.Path);
    }

    public async Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
    {
        try
        {
            var blob = GetBlobReference(path);

            if (!overwrite && await blob.ExistsAsync())
            {
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            }

            _contentTypeProvider.TryGetContentType(path, out var contentType);

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType ?? MediaTypeNames.Application.Octet,
            };

            await blob.UploadAsync(inputStream, headers);

            return path;
        }
        catch (FileStoreException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FileStoreException($"Cannot create file '{path}'.", ex);
        }
    }

    private BlobClient GetBlobReference(string path)
    {
        var blobPath = this.Combine(_options.BasePath, path);
        var blob = _blobContainer.GetBlobClient(blobPath);

        return blob;
    }

    private async Task<BlobHierarchyItem> GetBlobDirectoryReference(string path)
    {
        var prefix = this.Combine(_basePrefix, path);
        prefix = NormalizePrefix(prefix);

        // Directory exists if path contains any files.
        var page = _blobContainer.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.None, "/", prefix, CancellationToken.None);

        var enumerator = page.GetAsyncEnumerator();

        var result = await enumerator.MoveNextAsync();
        if (result)
        {
            return enumerator.Current;
        }

        return null;
    }

    private async Task CreateDirectoryAsync(string path)
    {
        var placeholderBlob = GetBlobReference(this.Combine(path, DirectoryMarkerFileName));

        // Create a directory marker file to make this directory appear when listing directories.
        using var stream = new MemoryStream(MarkerFileContent);

        await placeholderBlob.UploadAsync(stream);
    }

    private static StorageSharedKeyCredential ParseCredentialsFromConnectionString(string connectionString)
    {
        string accountName = null;
        string accountKey = null;

        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kvp = part.Split('=', 2);
            if (kvp.Length == 2)
            {
                if (kvp[0].Equals("AccountName", StringComparison.OrdinalIgnoreCase))
                {
                    accountName = kvp[1];
                }
                else if (kvp[0].Equals("AccountKey", StringComparison.OrdinalIgnoreCase))
                {
                    accountKey = kvp[1];
                }
            }
        }

        return new StorageSharedKeyCredential(
            accountName ?? throw new FileStoreException("AccountName not found in connection string."),
            accountKey ?? throw new FileStoreException("AccountKey not found in connection string."));
    }

    /// <summary>
    /// Blob prefix requires a trailing slash except when loading the root of the container.
    /// </summary>
    private static string NormalizePrefix(string prefix)
    {
        prefix = prefix.Trim('/') + '/';
        if (prefix.Length == 1)
        {
            return string.Empty;
        }
        else
        {
            return prefix;
        }
    }
}
