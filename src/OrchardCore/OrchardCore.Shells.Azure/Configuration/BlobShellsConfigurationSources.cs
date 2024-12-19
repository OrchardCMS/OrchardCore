using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Azure.Services;

namespace OrchardCore.Shells.Azure.Configuration;

public class BlobShellsConfigurationSources : IShellsConfigurationSources
{
    private static readonly string _appSettings =
        Path.GetFileNameWithoutExtension(OrchardCoreConstants.Configuration.ApplicationSettingsFileName);

    private readonly IShellsFileStore _shellsFileStore;
    private readonly BlobShellStorageOptions _blobOptions;

    private readonly string _environment;
    private readonly string _fileSystemAppSettings;

    public BlobShellsConfigurationSources(
        IShellsFileStore shellsFileStore,
        IHostEnvironment hostingEnvironment,
        BlobShellStorageOptions blobOptions,
        IOptions<ShellOptions> shellOptions
        )
    {
        _shellsFileStore = shellsFileStore;
        _environment = hostingEnvironment.EnvironmentName;
        _blobOptions = blobOptions;
        _fileSystemAppSettings = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, _appSettings);
    }

    public async Task AddSourcesAsync(IConfigurationBuilder builder)
    {
        var appSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync(OrchardCoreConstants.Configuration.ApplicationSettingsFileName);

        if (appSettingsFileInfo == null && _blobOptions.MigrateFromFiles)
        {
            if (await TryMigrateFromFileAsync($"{_fileSystemAppSettings}.json", OrchardCoreConstants.Configuration.ApplicationSettingsFileName))
            {
                appSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync(OrchardCoreConstants.Configuration.ApplicationSettingsFileName);
            }
        }

        if (appSettingsFileInfo != null)
        {
            var stream = await _shellsFileStore.GetFileStreamAsync(OrchardCoreConstants.Configuration.ApplicationSettingsFileName);
            builder.AddTenantJsonStream(stream);
        }

        var environmentAppSettingsFileName = $"{_appSettings}.{_environment}.json";
        var environmentAppSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync(environmentAppSettingsFileName);
        if (environmentAppSettingsFileInfo == null && _blobOptions.MigrateFromFiles)
        {
            if (await TryMigrateFromFileAsync($"{_fileSystemAppSettings}.{_environment}.json", environmentAppSettingsFileName))
            {
                environmentAppSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync(environmentAppSettingsFileName);
            }
            else
            {
                return;
            }
        }

        if (environmentAppSettingsFileInfo != null)
        {
            var stream = await _shellsFileStore.GetFileStreamAsync(environmentAppSettingsFileName);
            builder.AddTenantJsonStream(stream);
        }
    }

    private async Task<bool> TryMigrateFromFileAsync(string fileSystemPath, string destPath)
    {
        if (!File.Exists(fileSystemPath))
        {
            return false;
        }

        using var file = File.OpenRead(fileSystemPath);
        await _shellsFileStore.CreateFileFromStreamAsync(destPath, file);

        return true;
    }
}
