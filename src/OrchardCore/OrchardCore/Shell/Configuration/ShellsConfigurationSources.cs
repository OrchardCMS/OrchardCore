using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Shell.Configuration
{
    public class ShellsConfigurationSources : IShellsConfigurationSources
    {
        private readonly string _environment;
        private readonly string _appsettings;

        public ShellsConfigurationSources(IHostEnvironment hostingEnvironment, IOptions<ShellOptions> shellOptions)
        {
            _environment = hostingEnvironment.EnvironmentName;
            _appsettings = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "appsettings");
        }

        public Task AddSourcesAsync(IConfigurationBuilder builder)
        {
            builder
                .AddJsonFile($"{_appsettings}.json", optional: true)
                .AddJsonFile($"{_appsettings}.{_environment}.json", optional: true);

            return Task.CompletedTask;
        }
    }
}
