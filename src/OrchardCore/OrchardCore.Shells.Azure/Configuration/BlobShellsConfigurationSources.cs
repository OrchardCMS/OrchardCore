using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Azure.Services;

namespace OrchardCore.Shells.Azure.Configuration
{
    public class BlobShellsConfigurationSources : IShellsConfigurationSources
    {
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
            _fileSystemAppSettings = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "appsettings");
        }

        public async Task AddSourcesAsync(IConfigurationBuilder builder)
        {
            var appSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync("appsettings.json");

            if (appSettingsFileInfo == null && _blobOptions.MigrateFromFiles)
            {
                if (await TryMigrateFromFileAsync($"{_fileSystemAppSettings}.json", "appsettings.json"))
                {
                    appSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync("appsettings.json");
                }
            }

            if (appSettingsFileInfo != null)
            {
                var stream = await _shellsFileStore.GetFileStreamAsync("appsettings.json");
                builder.AddJsonStream(stream);
            }

            var environmentAppSettingsFileName = $"appsettings.{_environment}.json";
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
                builder.AddJsonStream(stream);
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
}
