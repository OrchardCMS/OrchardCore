using System;
using System.Linq;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobDirectory : IFileStoreEntry
    {
        private readonly string _path;
        private readonly DateTime _lastModifiedUtc;
        private readonly string _name;
        private readonly string _directoryPath;

        public BlobDirectory(string path, DateTime lastModifiedUtc)
        {
            _path = path;
            _lastModifiedUtc = lastModifiedUtc;
            // Use GetFileName rather than GetDirectoryName as GetDirectoryName requires a delimeter
            _name = System.IO.Path.GetFileName(path);
            _directoryPath = _path.Length > _name.Length ? _path.Substring(0, _path.Length - _name.Length - 1) : "";
        }

        public string Path => _path;

        public string Name => _name;

        public string DirectoryPath => _directoryPath;

        public long Length => 0;

        public DateTime LastModifiedUtc => _lastModifiedUtc;

        public bool IsDirectory => true;
    }
}
