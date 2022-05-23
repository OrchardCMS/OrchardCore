using System;

namespace OrchardCore.FileStorage.AmazonS3;

public class AwsFile : IFileStoreEntry
{
    private readonly string _path;
    private readonly string _name;
    private readonly string _directoryPath;
    private readonly long? _length;
    private readonly DateTimeOffset? _lastModified;

    public AwsFile(string path, long? length, DateTimeOffset? lastModified)
    {
        _path = path;
        _name = System.IO.Path.GetFileName(_path);

        if (_name == _path)
        {
            // File is in the root directory.
            _directoryPath = String.Empty;
        }
        else
        {
            _directoryPath = _path[..(_path.Length - _name.Length - 1)];
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
