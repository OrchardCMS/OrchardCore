using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly string _tenantsContainerPath;

        public ShellSettingsManager(
            IEnumerable<ITenantsConfigurationProvider> configurationProviders,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            _tenantsContainerPath = Path.Combine(
                options.Value.ShellsApplicationDataPath,
                options.Value.ShellsContainerName);

            Directory.CreateDirectory(_tenantsContainerPath);

            // We use root data (not in a section) as settings for all tenants. So, to have less data and prevent
            // name conflicts, we separate them from other app settings and always use a prefix for env variables.

            // To not interleave env variables bindings, a prefix should not start with another existing prefix.
            // When defining a child, to be compatible on all platforms, use a double underscore '__' seperator.

            var appsettingsPath = Path.Combine(options.Value.ShellsApplicationDataPath, "appsettings");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddEnvironmentVariables("ORCHARDCORE_SETTINGS_")
                .AddEnvironmentVariables($"ORCHARDCORE_{hostingEnvironment.EnvironmentName.ToUpperInvariant()}_SETTINGS_")
                .AddJsonFile($"{appsettingsPath}.json", optional: true)
                .AddJsonFile($"{appsettingsPath}.{hostingEnvironment.EnvironmentName}.json", optional: true);

            foreach (var configurationProvider in configurationProviders.OrderBy(p => p.Order))
            {
                configurationBuilder.AddConfiguration(configurationProvider.Configuration);
            }

            Configuration = configurationBuilder.Build();
        }

        public IConfiguration Configuration { get; }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            // Retrieve pre-configured tenants whose 'State' value is provided by the
            // 'Configuration' and resolve their folders even if they don't exist yet.

            var configuredTenantFolders = Configuration.GetChildren()
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => Path.Combine(_tenantsContainerPath, section.Key));

            // Add the folders of pre-configured tenants to the existing ones.
            var tenantFolders = Directory.GetDirectories(_tenantsContainerPath)
                .Concat(configuredTenantFolders).Distinct();

            var shellSettings = new List<ShellSettings>();

            foreach (var tenantFolder in tenantFolders)
            {
                var tenantName = Path.GetFileName(tenantFolder);

                var localConfiguration = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(tenantFolder, "appsettings.json"), optional: true)
                    .Build();

                var shellSetting = new ShellSettings() { Name = tenantName };

                // Apply root settings, then settings of the tenant section.
                // Then the only root settings of the local configuration.

                Configuration.Bind(shellSetting);
                Configuration.Bind(tenantName, shellSetting);
                localConfiguration.Bind(shellSetting);

                shellSettings.Add(shellSetting);
            }

            return shellSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var tenantFolder = Path.Combine(_tenantsContainerPath, settings.Name);
            Directory.CreateDirectory(tenantFolder);

            var globalSettings = new ShellSettings() { Name = settings.Name };

            // Apply root and section settings.
            Configuration.Bind(globalSettings);
            Configuration.Bind(settings.Name, globalSettings);

            var localObject = JObject.FromObject(settings);
            var globalObject = JObject.FromObject(globalSettings);

            foreach (var property in globalObject)
            {
                if (property.Key != "Name")
                {
                    var localValue = localObject.Value<string>(property.Key);
                    var globalValue = globalObject.Value<string>(property.Key);

                    // Only keep non null values and that override the global configuration.
                    // Allow e.g to setup a tenant in a pre-configured 'Uninitialized' state.

                    if (localValue == null || globalValue == localValue)
                    {
                        localObject.Remove(property.Key);
                    }
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(tenantFolder, "appsettings.json"), localObject.ToString());
            }

            catch (IOException)
            {
                // The settings file may be own by another process or already exists when trying to create
                // a new one. So, nothing more that we can do. Note: Other exceptions are normally thrown.

                // Tenant settings are not intended to be updated concurrently, but it is highly possible
                // when synchronizing tenants settings of multiple instances sharing the same file system.

                // Another potential issue is when one instance is reading a settings file, which can only
                // occur on startup, while another one is re-creating the settings file of the same tenant.
            }
        }
    }
}