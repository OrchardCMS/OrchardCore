namespace OrchardCore.Deployment.Core.Services;

public sealed class TemporaryFileBuilder : IFileBuilder, IDisposable
{
    public const string VirtualPathRequired = "A virtual path is required.";
    public const string PathMustBeRelative = "Path must be relative to the base directory.";
    
    private readonly bool _deleteOnDispose;

    public TemporaryFileBuilder(bool deleteOnDispose = true)
        : this(Path.GetTempPath(), deleteOnDispose)
    {
    }

    public TemporaryFileBuilder(string baseDirectory, bool deleteOnDispose = true)
    {
        Folder = PathExtensions.Combine(baseDirectory, Path.GetRandomFileName());
        _deleteOnDispose = deleteOnDispose;
    }

    public string Folder { get; }

    public void Dispose()
    {
        if (_deleteOnDispose)
        {
            if (Directory.Exists(Folder))
            {
                Directory.Delete(Folder, true);
            }
        }
    }

    public async Task SetFileAsync(string subpath, Stream stream)
    {
        if (subpath.StartsWith('/'))
        {
            throw new InvalidOperationException(VirtualPathRequired);
        }

        // Use "Path.GetFullPath" to resolve path segments like ".." and prevent more elaborate path traversal attacks.
        var fullname =  Path.GetFullPath(subpath, Folder);
        fullname = Path.GetFullPath(new Uri(fullname).LocalPath);

        var directory = new DirectoryInfo(Path.GetDirectoryName(fullname));

        if (Path.GetRelativePath(Folder, fullname).StartsWith(".."))
        {
            throw new InvalidOperationException(PathMustBeRelative);
        }

        if (!directory.Exists)
        {
            directory.Create();
        }

        using var fs = File.Create(fullname, 4 * 1024, FileOptions.None);
        await stream.CopyToAsync(fs);
    }

    public override string ToString()
    {
        return Folder;
    }
}
