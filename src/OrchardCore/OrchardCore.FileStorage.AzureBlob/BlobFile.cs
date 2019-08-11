using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobFile : IFileStoreEntry
    {
        private readonly string _path;
        private readonly BlobProperties _blobProperties;
        private readonly string _name;
        private readonly string _directoryPath;

        public BlobFile(string path, BlobProperties blobProperties)
        {
            _path = path;
            _blobProperties = blobProperties;
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

        public long Length => _blobProperties.Length;

        public DateTime LastModifiedUtc => _blobProperties.LastModified.GetValueOrDefault().UtcDateTime;

        public bool IsDirectory => false;
    }
}
