using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob;
using OrchardCore.StorageProviders;

namespace OrchardCore.Azure.Storage
{
    public class BlobDirectory : IFile
    {
        public BlobDirectory(CloudBlobDirectory blobDirectory)
        {
            _blobDirectory = blobDirectory;
        }

        private CloudBlobDirectory _blobDirectory;

        public string AbsolutePath => throw new NotImplementedException();

        public string Path => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Folder => throw new NotImplementedException();

        public long Length => throw new NotImplementedException();

        public DateTime LastModified => throw new NotImplementedException();

        public bool IsDirectory => throw new NotImplementedException();

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }
    }
}
