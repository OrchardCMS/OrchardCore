using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.FileStorage;

/// <summary>
/// Default filesystem based <see cref="ITempDirectoryProvider"/> implementation. Temporary files are placed under a
/// tenant-scoped sub-directory of <see cref="TempDirectoryOptions.Path"/> or the user's local application data
/// directory. A unique system temporary directory is used when no user location is available.
/// </summary>
public sealed class DefaultTempDirectoryProvider : ITempDirectoryProvider
{
    private const UnixFileMode OwnerOnly = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    private readonly string _rootPath;

    public DefaultTempDirectoryProvider(
        IOptions<TempDirectoryOptions> options,
        ShellSettings shellSettings)
        : this(options, shellSettings, NullLogger<DefaultTempDirectoryProvider>.Instance)
    {
    }

    public DefaultTempDirectoryProvider(
        IOptions<TempDirectoryOptions> options,
        ShellSettings shellSettings,
        ILogger<DefaultTempDirectoryProvider> logger)
    {
        var basePath = options.Value.Path;

        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = GetDefaultBasePath(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                logger);
        }

        // Scope temporary files per tenant so that tenants can neither collide with nor observe each other's files.
        _rootPath = Path.Combine(basePath, shellSettings.Name);
    }

    internal static string GetDefaultBasePath(string localAppDataPath, ILogger<DefaultTempDirectoryProvider> logger)
    {
        if (!string.IsNullOrWhiteSpace(localAppDataPath))
        {
            var path = Path.Combine(localAppDataPath, "OrchardCore", "Temp");

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    Directory.CreateDirectory(path, OwnerOnly);
                }

                return path;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return CreateFallbackDirectory(logger, ex);
            }
        }

        return CreateFallbackDirectory(logger);
    }

    private static string CreateFallbackDirectory(ILogger<DefaultTempDirectoryProvider> logger, Exception exception = null)
    {
        // CreateTempSubdirectory uses Path.GetTempPath(): normally the user's temp folder on Windows, or TMPDIR
        // or /tmp on Unix. Atomic creation with mode 700 prevents pre-creation attacks in sticky-bit-protected /tmp.
        var path = Directory.CreateTempSubdirectory("OrchardCore-").FullName;

        logger.LogWarning(
            exception,
            "No temporary directory path is configured and the user's local application data directory is unavailable. Using the unique temporary directory '{TempDirectoryPath}'.",
            path);

        return path;
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
            Directory.CreateDirectory(_rootPath, OwnerOnly);
        }
    }
}
