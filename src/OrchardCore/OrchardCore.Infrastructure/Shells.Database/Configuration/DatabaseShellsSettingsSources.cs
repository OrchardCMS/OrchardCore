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
    public class DatabaseShellsSettingsSources : IShellsSettingsSources
    {
        private readonly DatabaseShellsStorageOptions _options;
        private readonly IShellContextFactory _shellContextFactory;

        public DatabaseShellsSettingsSources(
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
                    $"The 'OrchardCore.Shells.Database' configuration section needs at least to define a 'DatabaseProvider'");
            }

            _shellContextFactory = shellContextFactory;
        }

        public async Task AddSourcesAsync(IConfigurationBuilder builder)
        {
            var context = GetContextAsync().GetAwaiter().GetResult();

            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var document = await session.Query<DatabaseShellsSettings>().FirstOrDefaultAsync();

                if (document == null)
                {
                    return;
                }

                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(document.ShellsSettings)));
            }
        }

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            var context = await GetContextAsync();

            using (var scope = context.ServiceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();

                var document = await session.Query<DatabaseShellsSettings>().FirstOrDefaultAsync();

                JObject tenantsSettings;
                if (document != null)
                {
                    tenantsSettings = JObject.Parse(document.ShellsSettings);
                }
                else
                {
                    document = new DatabaseShellsSettings();
                    tenantsSettings = new JObject();
                }

                var settings = tenantsSettings.GetValue(tenant) as JObject ?? new JObject();

                foreach (var key in data.Keys)
                {
                    if (data[key] != null)
                    {
                        settings[key] = data[key];
                    }
                    else
                    {
                        settings.Remove(key);
                    }
                }

                tenantsSettings[tenant] = settings;

                document.ShellsSettings = tenantsSettings.ToString();

                session.Save(document);
            }
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
