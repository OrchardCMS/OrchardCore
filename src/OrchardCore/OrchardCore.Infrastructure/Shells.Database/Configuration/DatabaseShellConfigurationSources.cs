using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using YesSql;

namespace OrchardCore.Shells.Database.Configuration
{
    public class DatabaseShellConfigurationSources : IShellConfigurationSources
    {
        private readonly DatabaseShellsStorageOptions _options;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly string _container;

        public DatabaseShellConfigurationSources(
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IShellContextFactory shellContextFactory,
            IOptions<ShellOptions> shellOptions)

        {
            _options = configuration
                .GetSection("OrchardCore")
                .GetSection("OrchardCore.Shells.Database")
                .Get<DatabaseShellsStorageOptions>();

            _shellContextFactory = shellContextFactory;

            _container = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, shellOptions.Value.ShellsContainerName);
        }

        public async Task AddSourcesAsync(string tenant, IConfigurationBuilder builder)
        {
            DatabaseShellConfigurations document;
            JObject configurations;

            using var context = await _shellContextFactory.GetDatabaseContextAsync(_options);
            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                document = await session.Query<DatabaseShellConfigurations>().FirstOrDefaultAsync();

                if (document != null)
                {
                    configurations = JObject.Parse(document.Configurations);
                }
                else
                {
                    document = new DatabaseShellConfigurations();
                    configurations = new JObject();
                }

                if (!configurations.ContainsKey(tenant))
                {
                    if (!_options.MigrateFromFiles)
                    {
                        return;
                    }

                    var configuration = await GetConfigurationFromFileAsync(tenant);

                    if (configuration == null)
                    {
                        return;
                    }

                    configurations[tenant] = JObject.Parse(configuration);
                    document.Configurations = configurations.ToString();

                    session.Save(document, checkConcurrency: true);
                }
            }

            var config = configurations.GetValue(tenant) as JObject;
            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(config.ToString())));
        }

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            using var context = await _shellContextFactory.GetDatabaseContextAsync(_options);
            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var document = await session.Query<DatabaseShellConfigurations>().FirstOrDefaultAsync();

                JObject configurations;
                if (document != null)
                {
                    configurations = JObject.Parse(document.Configurations);
                }
                else
                {
                    document = new DatabaseShellConfigurations();
                    configurations = new JObject();
                }

                var config = configurations.GetValue(tenant) as JObject ?? new JObject();

                foreach (var key in data.Keys)
                {
                    if (data[key] != null)
                    {
                        config[key] = data[key];
                    }
                    else
                    {
                        config.Remove(key);
                    }
                }

                configurations[tenant] = config;

                document.Configurations = configurations.ToString();

                session.Save(document);
            }
        }

        private async Task<string> GetConfigurationFromFileAsync(string tenant)
        {
            var tenantFolder = Path.Combine(_container, tenant);
            var appsettings = Path.Combine(tenantFolder, "appsettings.json");

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
