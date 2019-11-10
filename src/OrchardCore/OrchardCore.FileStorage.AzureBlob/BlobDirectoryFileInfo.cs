using System;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobDirectoryFileInfo : IFileInfo
    {
        private readonly CloudBlobDirectory _blobDirectory;

        public bool Exists => true;

        public bool IsDirectory => true;

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public long Length => -1;

        public string Name => _blobDirectory.Prefix.TrimEnd('/');

        public string PhysicalPath => null;

        public BlobDirectoryFileInfo(CloudBlobDirectory blobDirectory)
        {
            _blobDirectory = blobDirectory;
        }

        public Stream CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create a stream for a directory.");
        }
    }
}
