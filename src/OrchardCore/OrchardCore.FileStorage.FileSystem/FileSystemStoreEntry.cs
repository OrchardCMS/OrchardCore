using Microsoft.Extensions.FileProviders;

namespace OrchardCore.FileStorage.FileSystem;

public class FileSystemStoreEntry : IFileStoreEntry
{
    private readonly IFileInfo _fileInfo;
    private readonly string _path;

    internal FileSystemStoreEntry(string path, IFileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(fileInfo);

        _fileInfo = fileInfo;
        _path = path;
    }

    public string Path => _path;
    public string Name => _fileInfo.Name;
    public string DirectoryPath => _path[..^Name.Length].TrimEnd('/');
    public DateTime LastModifiedUtc => _fileInfo.LastModified.UtcDateTime;
    public long Length => _fileInfo.Length;
    public bool IsDirectory => _fileInfo.IsDirectory;
}
