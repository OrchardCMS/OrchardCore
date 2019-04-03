using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Shell.Configuration
{
    public class ShellsConfigurationSources : IShellsConfigurationSources
    {
        private readonly string _environment;
        private readonly string _appsettings;

        public ShellsConfigurationSources(IHostingEnvironment hostingEnvironment, IOptions<ShellOptions> shellOptions)
        {
            _environment = hostingEnvironment.EnvironmentName;
            _appsettings = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "appsettings");
        }

        public void AddSources(IConfigurationBuilder builder)
        {
            builder
                .AddJsonFile($"{_appsettings}.json", optional: true)
                .AddJsonFile($"{_appsettings}.{_environment}.json", optional: true);
        }
    }
}