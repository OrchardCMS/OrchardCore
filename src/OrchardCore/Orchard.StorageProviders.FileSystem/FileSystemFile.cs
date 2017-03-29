using System;
using System.IO;

namespace Orchard.StorageProviders.FileSystem
{
    public class FileSystemFile : IFile
    {
        private readonly FileSystemInfo _fileInfo;
        private readonly string _path;
        private readonly string _publicPathPrefix;

        public FileSystemFile(string path, string publicPathPrefix, FileSystemInfo fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _path = path.Replace("\\", "/");
            _publicPathPrefix = publicPathPrefix.Replace("\\", "/");
        }

        public string AbsolutePath => _publicPathPrefix + "/" + _path;
        public string Path => _path;
        public string Name => _fileInfo.Name;
        public string Folder => _path.Substring(0, _path.Length - Name.Length).TrimEnd('/');
        public DateTime LastModified => _fileInfo.LastWriteTimeUtc;
        public long Length => (_fileInfo as FileInfo)?.Length ?? 0;
        public bool IsDirectory => _fileInfo is DirectoryInfo;
        public Stream CreateReadStream() => (_fileInfo as FileInfo)?.OpenRead();
    }
}
