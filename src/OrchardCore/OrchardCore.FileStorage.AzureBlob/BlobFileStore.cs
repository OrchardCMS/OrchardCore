using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OrchardCore.Modules;

namespace OrchardCore.FileStorage.AzureBlob
{
    /// <summary>
    /// Provides an <see cref="IFileStore"/> implementation that targets an underlying Azure Blob Storage account.
    /// </summary>
    /// <remarks>
    /// Azure Blob Storage has very different semantics for directories compared to a local file system, and
    /// some special consideration is required for make this provider conform to the semantics of the
    /// <see cref="IFileStore"/> interface and behave in an expected way.
    /// 
    /// Directories have no physical manifestation in blob storage; we can obtain a reference to them, but
    /// that reference can be created regardless of whether the directory exists, and it can only be used
    /// as a scoping container to operate on blobs within that directory namespace.
    /// 
    /// As a consequence, this provider generally behaves as if any given directory always exists. To 
    /// simulate "creating" a directory (which cannot technically be done in blob storage) this provider creates
    /// a marker file inside the directory, which makes the directory "exist" and appear when listing contents
    /// subsequently. This marker file is ignored (excluded) when listing directory contents.
    /// </remarks>
    public class BlobFileStore : IFileStore
    {
        private const string _directoryMarkerFileName = "OrchardCore.Media.txt";

        private readonly BlobStorageOptions _options;
        private readonly IClock _clock;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _blobContainer;
        private readonly Task _verifyContainerTask;

        public BlobFileStore(BlobStorageOptions options, IClock clock)
        {
            _options = options;
            _clock = clock;
            _storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(_options.ContainerName);
            _verifyContainerTask = Task.Run(async () =>
            {
                try
                {
                    await _blobContainer.CreateIfNotExistsAsync();
                    await _blobContainer.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
                }
                catch (Exception ex)
                {
                    throw new FileStoreException($"Error while creating or setting permissions on container {_options.ContainerName}.", ex);
                }
            });
        }

        public Uri BaseUri
        {
            get
            {
                var uriBuilder = new UriBuilder(_blobContainer.Uri);
                uriBuilder.Path = this.Combine(uriBuilder.Path, _options.BasePath);
                return uriBuilder.Uri;
            }
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            await _verifyContainerTask;

            var blob = GetBlobReference(path);

            if (!await blob.ExistsAsync()) 
                return null;

            await blob.FetchAttributesAsync();

            return new BlobFile(path, blob.Properties);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            await _verifyContainerTask;

            var placeholderBlob = GetBlobReference(this.Combine(path, _directoryMarkerFileName));

            if (await placeholderBlob.ExistsAsync())
            {
                var blobDirectory = GetBlobDirectoryReference(path);
                return new BlobDirectory(path, _clock.UtcNow);
            }
            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = "")
        {
            await _verifyContainerTask;

            var blobDirectory = GetBlobDirectoryReference(path);

            BlobContinuationToken continuationToken = null;

            var results = new List<IFileStoreEntry>();

            do
            {
                var segment =
                    await blobDirectory.ListBlobsSegmentedAsync(
                        useFlatBlobListing: false,
                        blobListingDetails: BlobListingDetails.Metadata,
                        maxResults: null,
                        currentToken: continuationToken,
                        options: null,
                        operationContext: null);

                foreach (var item in segment.Results)
                {
                    var itemName = WebUtility.UrlDecode(item.Uri.Segments.Last());
                    var itemPath = this.Combine(path, itemName);

                    switch (item)
                    {
                        case CloudBlobDirectory directoryItem:
                            results.Add(new BlobDirectory(itemPath, _clock.UtcNow));
                            break;
                        case CloudBlockBlob blobItem:
                            // Ignore directory marker files.
                            if (itemName != _directoryMarkerFileName)
                                results.Add(new BlobFile(itemPath, blobItem.Properties));
                            break;
                    }
                }

                continuationToken = segment.ContinuationToken;
            }
            while (continuationToken != null);

            return
                results
                    .OrderByDescending(x => x.IsDirectory)
                    .ToArray();
        }

        public async Task<bool> TryCreateDirectoryAsync(string path)
        {
            await _verifyContainerTask;

            // Since directories are only created implicitly when creating blobs, we
            // simply pretend like we created the directory, unless there is already
            // a blob with the same path.

            var blob = GetBlobReference(path);

            if (await blob.ExistsAsync())
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            // Create a directory marker file to make this directory appear when
            // listing directories.
            var placeholderBlob = GetBlobReference(this.Combine(path, _directoryMarkerFileName));
            await placeholderBlob.UploadTextAsync("This is a directory marker file created by Orchard Core. It is safe to delete it.");

            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path)
        {
            await _verifyContainerTask;

            var blob = GetBlobReference(path);

            return await blob.DeleteIfExistsAsync();
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            await _verifyContainerTask;

            var blobDirectory = GetBlobDirectoryReference(path);

            BlobContinuationToken continuationToken = null;
            var blobsWereDeleted = false;

            do
            {
                var segment = 
                    await blobDirectory.ListBlobsSegmentedAsync(
                        useFlatBlobListing: true, 
                        blobListingDetails: BlobListingDetails.None, 
                        maxResults: null, 
                        currentToken: continuationToken, 
                        options: null, 
                        operationContext: null);

                foreach (var item in segment.Results)
                {
                    if (item is CloudBlob blob)
                    {
                        await blob.DeleteAsync();
                        blobsWereDeleted = true;
                    }
                }

                continuationToken = segment.ContinuationToken;
            }
            while (continuationToken != null);

            return blobsWereDeleted;
        }

        public async Task MoveFileAsync(string oldPath, string newPath)
        {
            await _verifyContainerTask;

            await CopyFileAsync(oldPath, newPath);
            await TryDeleteFileAsync(oldPath);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath)
        {
            if (srcPath == dstPath)
                throw new ArgumentException($"The values for {nameof(srcPath)} and {nameof(dstPath)} must not be the same.");

            await _verifyContainerTask;

            var oldBlob = GetBlobReference(srcPath);
            var newBlob = GetBlobReference(dstPath);

            if (!await oldBlob.ExistsAsync())
                throw new FileStoreException($"Cannot copy file '{srcPath}' because it does not exist.");

            if (await newBlob.ExistsAsync())
                throw new FileStoreException($"Cannot copy file '{srcPath}' because a file already exists in the new path '{dstPath}'.");

            var operationId = await newBlob.StartCopyAsync(oldBlob);

            while (newBlob.CopyState.Status == CopyStatus.Pending)
                await Task.Delay(250);

            if (newBlob.CopyState.Status != CopyStatus.Success)
                throw new FileStoreException($"Error while copying file '{srcPath}'; copy operation failed with status {newBlob.CopyState.Status} and description {newBlob.CopyState.StatusDescription}.");
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            await _verifyContainerTask;

            var blob = GetBlobReference(path);

            if (!await blob.ExistsAsync())
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return await blob.OpenReadAsync();
        }

        public async Task CreateFileFromStream(string path, Stream inputStream, bool overwrite = false)
        {
            await _verifyContainerTask;

            var blob = GetBlobReference(path);

            if (!overwrite && await blob.ExistsAsync())
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            await blob.UploadFromStreamAsync(inputStream);
        }

        private CloudBlockBlob GetBlobReference(string path)
        {
            var blobPath = this.Combine(_options.BasePath, path);
            var blob = _blobContainer.GetBlockBlobReference(blobPath);

            return blob;
        }

        private CloudBlobDirectory GetBlobDirectoryReference(string path)
        {
            var blobDirectoryPath = this.Combine(_options.BasePath, path);
            var blobDirectory = _blobContainer.GetDirectoryReference(blobDirectoryPath);

            return blobDirectory;
        }
    }
}
