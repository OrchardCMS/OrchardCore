using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using YesSql;

namespace OrchardCore.Shells.Database.Configuration
{
    public class DatabaseShellsConfigurationSources : IShellsConfigurationSources
    {
        private const string _allEnvironments = "AllEnvironments";

        private readonly DatabaseShellsStorageOptions _options;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly string _environment;
        private readonly string _appsettings;

        public DatabaseShellsConfigurationSources(
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IShellContextFactory shellContextFactory,
            IHostEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions)

        {
            _options = configuration
                .GetSection("OrchardCore")
                .GetSection("OrchardCore.Shells.Database")
                .Get<DatabaseShellsStorageOptions>();

            _shellContextFactory = shellContextFactory;

            _environment = hostingEnvironment.EnvironmentName;
            _appsettings = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "appsettings");
        }

        public async Task AddSourcesAsync(IConfigurationBuilder builder)
        {
            DatabaseShellsConfigurations document;
            JObject configurations;

            using var context = await _shellContextFactory.GetDatabaseContextAsync(_options);
            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                document = await session.Query<DatabaseShellsConfigurations>().FirstOrDefaultAsync();

                if (document != null)
                {
                    configurations = JObject.Parse(document.Configurations);
                }
                else
                {
                    document = new DatabaseShellsConfigurations();
                    configurations = new JObject();
                }

                if (!configurations.ContainsKey(_allEnvironments) && _options.MigrateFromFiles)
                {
                    var configuration = await GetConfigurationFromFileAsync($"{_appsettings}.json");

                    if (configuration != null)
                    {
                        configurations[_allEnvironments] = JObject.Parse(configuration);
                        document.Configurations = configurations.ToString();
                        session.Save(document, checkConcurrency: true);
                    }
                }

                if (!configurations.ContainsKey(_environment) && _options.MigrateFromFiles)
                {
                    var configuration = await GetConfigurationFromFileAsync($"{_appsettings}.{_environment}.json");

                    if (configuration != null)
                    {
                        configurations[_environment] = JObject.Parse(configuration);
                        document.Configurations = configurations.ToString();
                        session.Save(document, checkConcurrency: true);
                    }
                }
            }

            var config = configurations.GetValue(_allEnvironments) as JObject;

            if (config != null)
            {
                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(config.ToString())));
            }

            config = configurations.GetValue(_environment) as JObject;

            if (config != null)
            {
                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(config.ToString())));
            }
        }

        private async Task<string> GetConfigurationFromFileAsync(string appsettings)
        {
            if (!File.Exists(appsettings))
            {
                return null;
            }

            using (var file = File.OpenText(appsettings))
            {
                var configuration = await file.ReadToEndAsync();
                return configuration;
            }
        }
    }
}
