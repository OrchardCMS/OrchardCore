using System.Runtime.Versioning;
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
    private const UnixFileMode OwnerOnly = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    private readonly string _rootPath;
    private readonly bool _isSharedSystemTempPath;
    private bool _rootVerified;

    public DefaultTempDirectoryProvider(
        IOptions<TempDirectoryOptions> options,
        ShellSettings shellSettings)
    {
        var basePath = options.Value.Path;

        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Path.GetTempPath();
            _isSharedSystemTempPath = true;
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

            return;
        }

        Directory.CreateDirectory(_rootPath, OwnerOnly);

        if (_isSharedSystemTempPath && !_rootVerified)
        {
            EnsureRootIsPrivate();
            _rootVerified = true;
        }
    }

    // On Unix the system temporary directory (usually '/tmp') is writable by every local user, and the tenant root
    // has a predictable name. 'Directory.CreateDirectory' does nothing when the path already exists, so another user
    // could create it first and then read, replace or redirect the files written to it, such as the recipes of an
    // imported deployment package. Make sure the root is a real directory that only the current user can access.
    [UnsupportedOSPlatform("windows")]
    private void EnsureRootIsPrivate()
    {
        var directory = new DirectoryInfo(_rootPath);

        if (directory.LinkTarget is not null)
        {
            throw new InvalidOperationException(
                $"The temporary directory '{_rootPath}' is a symbolic link. Remove it, or configure a private " +
                "location with the 'OrchardCore:TempDirectory:Path' setting.");
        }

        try
        {
            // Changing the mode of a directory requires owning it, so this fails when it was created by another
            // user, and removes any access a pre-existing directory may have granted to other users.
            File.SetUnixFileMode(_rootPath, OwnerOnly);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException(
                $"The temporary directory '{_rootPath}' is owned by another user. Remove it, or configure a private " +
                "location with the 'OrchardCore:TempDirectory:Path' setting.",
                ex);
        }
    }
}
