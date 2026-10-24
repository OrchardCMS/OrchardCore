using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.FileStorage;

/// <summary>
/// Default filesystem based <see cref="ITempDirectoryProvider"/> implementation. Temporary files are placed under a
/// tenant-scoped sub-directory of <see cref="TempDirectoryOptions.Path"/> or a unique sub-directory of the operating
/// system temporary directory when no path is configured.
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
            var systemTempPath = Path.GetTempPath();

            if (!string.IsNullOrWhiteSpace(systemTempPath) && Directory.Exists(systemTempPath))
            {
                basePath = Directory.CreateTempSubdirectory($"OrchardCore-{Guid.NewGuid():N}-").FullName;

                logger.LogWarning(
                    "No temporary directory path is configured. Using the unique temporary directory '{TempDirectoryPath}'.",
                    basePath);
            }
            else
            {
                var localAppDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrWhiteSpace(localAppDataPath))
                {
                    throw new InvalidOperationException(
                        $"The operating system temporary directory '{systemTempPath}' is unavailable and no local application data directory could be determined. Configure 'OrchardCore:TempDirectory:Path'.");
                }

                basePath = Path.Combine(localAppDataPath, "OrchardCore", "Temp", Guid.NewGuid().ToString("N"));
                if (OperatingSystem.IsWindows())
                {
                    Directory.CreateDirectory(basePath);
                }
                else
                {
                    Directory.CreateDirectory(basePath, OwnerOnly);
                }

                logger.LogWarning(
                    "No temporary directory path is configured and the operating system temporary directory '{SystemTempPath}' is unavailable. Using the unique temporary directory '{TempDirectoryPath}'.",
                    systemTempPath,
                    basePath);
            }
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
            Directory.CreateDirectory(_rootPath, OwnerOnly);
        }
    }
}
