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

        public void AddSources(IConfigurationBuilder builder)
        {
            var appSettingsFileInfo = _shellsFileStore.GetFileInfoAsync("appsettings.json").GetAwaiter().GetResult();
            if (appSettingsFileInfo != null)
            {
                var stream = _shellsFileStore.GetFileStreamAsync("appsettings.json").GetAwaiter().GetResult();
                builder.AddJsonStream(stream);
            }

            var environmentAppSettingsFileName = $"appsettings.{_environment}.json";
            var environmentAppSettingsFileInfo = _shellsFileStore.GetFileInfoAsync(environmentAppSettingsFileName).GetAwaiter().GetResult();
            if (environmentAppSettingsFileInfo != null)
            {
                var stream = _shellsFileStore.GetFileStreamAsync(environmentAppSettingsFileName).GetAwaiter().GetResult();
                builder.AddJsonStream(stream);
            }
        }
    }
}
