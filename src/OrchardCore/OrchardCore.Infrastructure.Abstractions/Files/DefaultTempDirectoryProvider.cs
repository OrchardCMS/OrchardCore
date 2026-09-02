using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.FileStorage;

/// <summary>
/// Default filesystem based <see cref="ITempDirectoryProvider"/> implementation. Temporary files are placed under a
/// tenant-scoped sub-directory of <see cref="TempDirectoryOptions.Path"/> (or the operating system
/// temporary directory when no path is configured).
/// </summary>
public sealed class DefaultTempDirectoryProvider : ITempDirectoryProvider
{
    private readonly string _rootPath;

    public DefaultTempDirectoryProvider(
        IOptions<TempDirectoryOptions> options,
        ShellSettings shellSettings)
    {
        var basePath = options.Value.Path;

        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Path.GetTempPath();
        }

        // Scope temporary files per tenant so that tenants can neither collide with nor observe each other's files.
        _rootPath = Path.Combine(basePath, shellSettings.Name);
    }

    public string GetRootDirectory()
    {
        CreateRootDirectory();

        return _rootPath;
    }

    public string CreateTempSubdirectory(string prefix = null)
    {
        CreateRootDirectory();

        string path;

        do
        {
            path = Path.Combine(_rootPath, prefix + Path.GetRandomFileName());
        }
        while (Directory.Exists(path) || File.Exists(path));

        Directory.CreateDirectory(path);

        return path;
    }

    public string GetTempFileName(string extension = null)
    {
        CreateRootDirectory();

        var name = Path.GetRandomFileName();

        if (!string.IsNullOrEmpty(extension))
        {
            name = Path.ChangeExtension(name, extension);
        }

        return Path.Combine(_rootPath, name);
    }

    private void CreateRootDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(_rootPath);
        }
        else
        {
            Directory.CreateDirectory(
                _rootPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }
}
