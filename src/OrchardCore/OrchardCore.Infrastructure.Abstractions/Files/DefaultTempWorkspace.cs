using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.FileStorage;

/// <summary>
/// Default filesystem based <see cref="ITempWorkspace"/> implementation. Temporary files are placed under a
/// tenant-scoped sub-directory of <see cref="TempWorkspaceOptions.TempPath"/> (or the operating system
/// temporary directory when no path is configured).
/// </summary>
public sealed class DefaultTempWorkspace : ITempWorkspace
{
    private readonly string _rootPath;

    public DefaultTempWorkspace(
        IOptions<TempWorkspaceOptions> options,
        ShellSettings shellSettings)
    {
        var basePath = options.Value.TempPath;

        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Path.GetTempPath();
        }

        // Scope temporary files per tenant so that tenants can neither collide with nor observe each other's files.
        _rootPath = Path.Combine(basePath, shellSettings.Name);
    }

    public string GetRootDirectory()
    {
        Directory.CreateDirectory(_rootPath);

        return _rootPath;
    }

    public string GetOrCreateSubdirectory(string name)
    {
        var path = GetSafeFullPath(name);

        Directory.CreateDirectory(path);

        return path;
    }

    public string GetTempFileName(string extension = null)
    {
        Directory.CreateDirectory(_rootPath);

        var name = Path.GetRandomFileName();

        if (!string.IsNullOrEmpty(extension))
        {
            name = Path.ChangeExtension(name, extension);
        }

        return Path.Combine(_rootPath, name);
    }

    public string CreateTempSubdirectory()
    {
        Directory.CreateDirectory(_rootPath);

        string path;

        do
        {
            path = Path.Combine(_rootPath, Path.GetRandomFileName());
        }
        while (Directory.Exists(path) || File.Exists(path));

        Directory.CreateDirectory(path);

        return path;
    }

    private string GetSafeFullPath(string relativePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);

        var rootFullPath = Path.GetFullPath(_rootPath);
        var fullPath = Path.GetFullPath(Path.Combine(rootFullPath, relativePath));

        // Guard against path traversal (e.g. "../../etc") escaping the tenant-scoped root.
        if (!string.Equals(fullPath, rootFullPath, StringComparison.Ordinal) &&
            !fullPath.StartsWith(rootFullPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The path '{relativePath}' resolves outside of the temporary file store root.");
        }

        return fullPath;
    }
}
