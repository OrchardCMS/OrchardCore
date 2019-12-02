using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
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
    ///
    /// Note that the Blob Container is not created automatically, and existence of the Container is not verified.
    /// 
    /// Create the Blob Container before enabling a Blob File Store.
    /// 
    /// Azure Blog Storage will create the BasePath inside the container during the upload of the first file.
    /// </remarks>
    public class BlobFileStore : IFileStore
    {
        private const string _directoryMarkerFileName = "OrchardCore.Media.txt";

        private readonly BlobStorageOptions _options;
        private readonly IClock _clock;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _blobContainer;
        private readonly IContentTypeProvider _contentTypeProvider;

        public BlobFileStore(BlobStorageOptions options, IClock clock, IContentTypeProvider contentTypeProvider)
        {
            _options = options;
            _clock = clock;
            _contentTypeProvider = contentTypeProvider;
            _storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(_options.ContainerName);
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
            var blob = GetBlobReference(path);

            if (!await blob.ExistsAsync())
            {
                return null;
            }

            await blob.FetchAttributesAsync();

            return new BlobFile(path, blob);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            var blobDirectory = GetBlobDirectoryReference(path);

            if (path == string.Empty || await BlobDirectoryExists(blobDirectory))
            {
                return new BlobDirectory(path, _clock.UtcNow);
            }

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = "", bool includeSubDirectories = false)
        {
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
                            if (includeSubDirectories || itemName != _directoryMarkerFileName)
                            {
                                results.Add(new BlobFile(itemPath, blobItem));
                            }
                            break;
                    }
                }

                continuationToken = segment.ContinuationToken;
            }
            while (continuationToken != null);

            return results
                    .OrderByDescending(x => x.IsDirectory)
                    .ToArray();
        }

        public async Task<bool> TryCreateDirectoryAsync(string path)
        {
            // Since directories are only created implicitly when creating blobs, we
            // simply pretend like we created the directory, unless there is already
            // a blob with the same path.

            var blob = GetBlobReference(path);

            if (await blob.ExistsAsync())
            {
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");
            }

            await CreateDirectoryAsync(path);

            return true;
        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            var blob = GetBlobReference(path);

            return blob.DeleteIfExistsAsync();
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new FileStoreException("Cannot delete the root directory.");
            }

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
            await CopyFileAsync(oldPath, newPath);
            await TryDeleteFileAsync(oldPath);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath)
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

            await newBlob.StartCopyAsync(oldBlob);

            while (newBlob.CopyState.Status == CopyStatus.Pending)
            {
                await Task.Delay(250);
                // Need to fetch or CopyState will never update.
                await newBlob.FetchAttributesAsync();
            }

            if (newBlob.CopyState.Status != CopyStatus.Success)
            {
                throw new FileStoreException($"Error while copying file '{srcPath}'; copy operation failed with status {newBlob.CopyState.Status} and description {newBlob.CopyState.StatusDescription}.");
            }
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            var blob = GetBlobReference(path);

            if (!await blob.ExistsAsync())
            {
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");
            }

            return await blob.OpenReadAsync();
        }

        // Reduces the need to call blob.FetchAttributes, and blob.ExistsAsync,
        // as Azure Storage Library will perform these actions on OpenReadAsync(). 
        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            var blobFile = fileStoreEntry as BlobFile;
            if (blobFile == null || blobFile.BlobReference == null)
            {
                throw new FileStoreException("Cannot get file stream because the file does not exist.");
            }

            return blobFile.BlobReference.OpenReadAsync();
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            var blob = GetBlobReference(path);

            if (!overwrite && await blob.ExistsAsync())
            {
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            }

            _contentTypeProvider.TryGetContentType(path, out var contentType);

            blob.Properties.ContentType = contentType ?? "application/octet-stream";

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

        private Task CreateDirectoryAsync(string path)
        {
            var placeholderBlob = GetBlobReference(this.Combine(path, _directoryMarkerFileName));

            // Create a directory marker file to make this directory appear when
            // listing directories.
            return placeholderBlob.UploadTextAsync(
                "This is a directory marker file created by Orchard Core. It is safe to delete it.");
        }

        private async Task<bool> BlobDirectoryExists(CloudBlobDirectory blobDirectory)
        {
            // CloudBlobDirectory exists if it has at least one blob
            BlobContinuationToken continuationToken = null;
            var segment = await blobDirectory.ListBlobsSegmentedAsync(
                useFlatBlobListing: false,
                blobListingDetails: BlobListingDetails.None,
                maxResults: 1,
                currentToken: continuationToken,
                options: null,
                operationContext: null);

            return segment.Results.Any();
        }
    }
}
