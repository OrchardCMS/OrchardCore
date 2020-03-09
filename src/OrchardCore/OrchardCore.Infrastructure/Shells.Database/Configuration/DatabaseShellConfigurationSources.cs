using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using YesSql;

namespace OrchardCore.Shells.Database.Configuration
{
    public class DatabaseShellConfigurationSources : IShellConfigurationSources
    {
        private readonly DatabaseShellsStorageOptions _options;
        private readonly IShellContextFactory _shellContextFactory;

        public DatabaseShellConfigurationSources(
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IShellContextFactory shellContextFactory)

        {
            _options = configuration
                .GetSection("OrchardCore")
                .GetSection("OrchardCore.Shells.Database")
                .Get<DatabaseShellsStorageOptions>();

            if (_options.DatabaseProvider == null)
            {
                throw new ArgumentNullException(nameof(_options.DatabaseProvider),
                    "The 'OrchardCore.Shells.Database' configuration section needs at least to define a 'DatabaseProvider'");
            }

            _shellContextFactory = shellContextFactory;
        }

        public async Task AddSourcesAsync(string tenant, IConfigurationBuilder builder)
        {
            var context =  await GetContextAsync();

            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var document = await session.Query<DatabaseShellConfigurations>().FirstOrDefaultAsync();

                if (document == null)
                {
                    return;
                }

                var configurations = JObject.Parse(document.ShellConfigurations);

                var config = configurations.GetValue(tenant) as JObject ?? new JObject();

                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(config.ToString())));
            }

            context.Release();
        }

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            var context = await GetContextAsync();

            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var document = await session.Query<DatabaseShellConfigurations>().FirstOrDefaultAsync();

                JObject configurations;
                if (document != null)
                {
                    configurations = JObject.Parse(document.ShellConfigurations);
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

                document.ShellConfigurations = configurations.ToString();

                session.Save(document);
            }

            context.Release();
        }

        private Task<ShellContext> GetContextAsync()
        {
            var settings = new ShellSettings()
            {
                Name = ShellHelper.DefaultShellName,
                State = TenantState.Running
            };

            settings["DatabaseProvider"] = _options.DatabaseProvider;
            settings["ConnectionString"] = _options.ConnectionString;
            settings["TablePrefix"] = _options.TablePrefix;

            return _shellContextFactory.CreateDescribedContextAsync(settings, new ShellDescriptor());
        }
    }
}
