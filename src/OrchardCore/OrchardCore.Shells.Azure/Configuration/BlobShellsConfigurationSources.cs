using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Azure.Services;

namespace OrchardCore.Shells.Azure.Configuration
{
    public class BlobShellsConfigurationSources : IShellsConfigurationSources
    {
        private readonly IShellsFileStore _shellsFileStore;

        private readonly string _environment;

        public BlobShellsConfigurationSources(IShellsFileStore shellsFileStore, IHostEnvironment hostingEnvironment)
        {
            _shellsFileStore = shellsFileStore;
            _environment = hostingEnvironment.EnvironmentName;
        }

        public async Task AddSourcesAsync(IConfigurationBuilder builder)
        {
            var appSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync("appsettings.json");
            if (appSettingsFileInfo != null)
            {
                var stream = await _shellsFileStore.GetFileStreamAsync("appsettings.json");
                builder.AddJsonStream(stream);
            }

            var environmentAppSettingsFileName = $"appsettings.{_environment}.json";
            var environmentAppSettingsFileInfo = await _shellsFileStore.GetFileInfoAsync(environmentAppSettingsFileName);
            if (environmentAppSettingsFileInfo != null)
            {
                var stream = await _shellsFileStore.GetFileStreamAsync(environmentAppSettingsFileName);
                builder.AddJsonStream(stream);
            }
        }
    }
}
