using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrchardCore.FileStorage.AliYunOss
{
    public class OssDirectory : IFileStoreEntry
    {
        private readonly string _path;
        private readonly DateTime _lastModifiedUtc;
        private readonly string _name;
        private readonly string _directoryPath;

        public OssDirectory(string path, DateTime lastModifiedUtc)
        {
            _path = path;
            _lastModifiedUtc = lastModifiedUtc;
            _name = _path.Split('/').Last();
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
