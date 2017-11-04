using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OrchardCore.Azure.Storage.Media;
using OrchardCore.Media;
using OrchardCore.StorageProviders;

namespace OrchardCore.Azure.Storage
{
    public class BlobFileStore : IFileStore
    {
        private readonly BlobStorageOptions _options;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _blobContainer;

        public BlobFileStore(BlobStorageOptions options)
        {
            _options = options;

            _storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(_options.ContainerName);

            // TODO: Ensure container exists, and set access permissions.
        }

        public string Combine(params string[] paths)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IFile>> GetDirectoryContentAsync(string subpath = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IFile> GetFileAsync(string subpath)
        {
            var blob = GetBlobReference(subpath);

            // TODO: Check if this is necessary, or if FetchAttributesAsync() can be used gracefully even if blob doesn't exist.
            if (!await blob.ExistsAsync()) 
                return null;

            await blob.FetchAttributesAsync();

            return new BlobFile(blob);
        }

        public Task<IFile> GetFolderAsync(string subpath)
        {
            var blobDirectory = GetBlobDirectoryReference(subpath);

            // TODO: How can we return null here if the directory does not exist, in a performant way?

            return Task.FromResult<IFile>(new BlobDirectory(blobDirectory));
        }


        public string GetPublicUrl(string subpath)
        {
            var blob = GetBlobReference(subpath);

            var uriBuilder = new UriBuilder(blob.Uri);

            //if (!string.IsNullOrEmpty(_options.PublicHostName))
            //    uriBuilder.Host = _options.PublicHostName;

            return uriBuilder.Uri.ToString();
        }

        public Task<IFile> MapFileAsync(string absoluteUrl)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryCopyFileAsync(string originalPath, string duplicatePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryCreateFolderAsync(string subpath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryDeleteFileAsync(string subpath)
        {
            var blob = GetBlobReference(subpath);

            return blob.DeleteIfExistsAsync();
        }

        public async Task<bool> TryDeleteFolderAsync(string subpath)
        {
            var blobDirectory = GetBlobDirectoryReference(subpath);

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

        public Task<bool> TryMoveFileAsync(string oldPath, string newPath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryMoveFolderAsync(string oldPath, string newPath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TrySaveStreamAsync(string subpath, Stream inputStream)
        {
            throw new NotImplementedException();
        }

        private CloudBlockBlob GetBlobReference(string subpath)
        {
            var blobPath = Combine(_options.BasePath, subpath);
            var blob = _blobContainer.GetBlockBlobReference(blobPath);

            return blob;
        }

        private CloudBlobDirectory GetBlobDirectoryReference(string subpath)
        {
            var blobDirectoryPath = Combine(_options.BasePath, subpath);
            var blobDirectory = _blobContainer.GetDirectoryReference(blobDirectoryPath);

            return blobDirectory;
        }
    }
}
