using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobFileProvider : IFileProvider
    {
        private readonly BlobStorageOptions _options;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _blobContainer;

        public BlobFileProvider(BlobStorageOptions options)
        {
            _options = options;

            _storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(_options.ContainerName);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var blob = _blobContainer.GetDirectoryReference(GetPath(subpath));
            return new BlobDirectoryContents(blob);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var blob = _blobContainer.GetBlockBlobReference(GetPath(subpath));
            return new BlockBlobFileInfo(blob);
        }

        public IChangeToken Watch(string filter)
        {
            // Changes are not handled for now.
            return NullChangeToken.Singleton;
        }

        private string GetPath(string subpath)
        {
            subpath = subpath?.TrimStart('/');

            if (!string.IsNullOrEmpty(_options.BasePath))
            {
                return _options.BasePath.TrimEnd('/') + '/' + subpath;
            }
            else
            {
                return subpath;
            }
        }
    }
}
