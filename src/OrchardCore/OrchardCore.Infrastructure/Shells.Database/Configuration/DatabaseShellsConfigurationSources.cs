using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using YesSql;

namespace OrchardCore.Shells.Database.Configuration
{
    public class DatabaseShellsConfigurationSources : IShellsConfigurationSources
    {
        private const string Any = "Any";

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
            JObject configurations;

            using var context = await _shellContextFactory.GetDatabaseContextAsync(_options);
            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var document = await session.Query<DatabaseShellsConfigurations>().FirstOrDefaultAsync();

                if (document != null)
                {
                    configurations = JObject.Parse(document.Configurations);
                }
                else
                {
                    document = new DatabaseShellsConfigurations();
                    configurations = new JObject();
                }

                if (!configurations.ContainsKey(Any) && _options.MigrateFromFiles &&
                    await TryMigrateFromFileAsync(Any, configurations))
                {
                    document.Configurations = configurations.ToString(Formatting.None);
                    session.Save(document);
                }

                if (!configurations.ContainsKey(_environment) && _options.MigrateFromFiles &&
                    await TryMigrateFromFileAsync(_environment, configurations))
                {
                    document.Configurations = configurations.ToString(Formatting.None);
                    session.Save(document);
                }
            }

            var configuration = configurations.GetValue(Any) as JObject;

            if (configuration != null)
            {
                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration.ToString(Formatting.None))));
            }

            configuration = configurations.GetValue(_environment) as JObject;

            if (configuration != null)
            {
                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configuration.ToString(Formatting.None))));
            }
        }

        private async Task<bool> TryMigrateFromFileAsync(string environment, JObject configurations)
        {
            var appsettings = _appsettings;

            if (environment != Any)
            {
                appsettings += "." + environment;
            }

            appsettings += ".json";

            if (!File.Exists(appsettings))
            {
                return false;
            }

            using (var file = File.OpenText(appsettings))
            {
                var configuration = await file.ReadToEndAsync();

                if (configuration != null)
                {
                    configurations[environment] = JObject.Parse(configuration);
                }
            }

            return true;
        }
    }
}
