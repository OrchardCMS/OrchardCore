using System;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobFile : IFileStoreEntry
    {
        private readonly string _path;
        private readonly string _name;
        private readonly string _directoryPath;
        private readonly long? _length;
        private readonly DateTimeOffset? _lastModified;

        public BlobFile(string path, long? length, DateTimeOffset? lastModified)
        {
            _path = path;
            _name = System.IO.Path.GetFileName(_path);

            // File is in root Directory.
            if (_name == _path)
            {
                _directoryPath = "";
            }
            else
            {
                _directoryPath = _path[..^(_name.Length + 1)];
            }

            _length = length;
            _lastModified = lastModified;
        }

        public string Path => _path;

        public string Name => _name;

        public string DirectoryPath => _directoryPath;

        public long Length => _length.GetValueOrDefault();

        public DateTime LastModifiedUtc => _lastModified.GetValueOrDefault().UtcDateTime;

        public bool IsDirectory => false;
    }
}
