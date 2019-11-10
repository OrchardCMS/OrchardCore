using System;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlockBlobFileInfo : IFileInfo
    {
        private readonly CloudBlockBlob _blockBlob;

        public bool Exists => _blockBlob.Exists();

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => _blockBlob.Properties.LastModified ?? DateTimeOffset.MinValue;

        public long Length => _blockBlob.Properties.Length;

        public string Name => _blockBlob.Name.Substring(_blockBlob.Parent.Prefix.Length);

        public string PhysicalPath => null;

        public BlockBlobFileInfo(CloudBlockBlob blockBlob)
        {
            _blockBlob = blockBlob;
        }

        public Stream CreateReadStream()
        {
            return _blockBlob.OpenRead();
        }
    }
}
