using System;
using Microsoft.Azure.Storage.Blob;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobFile : IFileStoreEntry
    {
        private readonly string _path;
        private readonly CloudBlockBlob _blobReference;
        private readonly string _name;
        private readonly string _directoryPath;

        public BlobFile(string path, CloudBlockBlob blobReference)
        {
            _path = path;
            _blobReference = blobReference;
            _name = System.IO.Path.GetFileName(_path);

            if (_name == _path) // file is in root Directory
            {
                _directoryPath = "";
            }
            else
            {
                _directoryPath = _path.Substring(0, _path.Length - _name.Length - 1);
            }
        }


        public string Path => _path;

        public string Name => _name;

        public string DirectoryPath => _directoryPath;

        public long Length => _blobReference.Properties.Length;

        public DateTime LastModifiedUtc => _blobReference.Properties.LastModified.GetValueOrDefault().UtcDateTime;

        public bool IsDirectory => false;

        public CloudBlockBlob BlobReference => _blobReference;
    }
}
