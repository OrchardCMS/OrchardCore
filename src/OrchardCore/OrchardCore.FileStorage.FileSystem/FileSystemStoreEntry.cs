using System;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.FileStorage.FileSystem
{
    public class FileSystemStoreEntry : IFileStoreEntry
    {
        private readonly IFileInfo _fileInfo;

        internal FileSystemStoreEntry(string path, IFileInfo fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path { get; }
        public string Name => _fileInfo.Name;
        public string DirectoryPath => Path.Substring(0, Path.Length - Name.Length).TrimEnd('/');
        public DateTime LastModifiedUtc => _fileInfo.LastModified.UtcDateTime;
        public long Length => _fileInfo.Length;
        public bool IsDirectory => _fileInfo.IsDirectory;
    }
}
