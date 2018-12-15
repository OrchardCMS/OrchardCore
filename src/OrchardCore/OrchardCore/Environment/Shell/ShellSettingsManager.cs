using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly string _tenantsContainerPath;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IEnumerable<ITenantsConfigurationSource> configurationSources,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            _tenantsContainerPath = Path.Combine(
                options.Value.ShellsApplicationDataPath,
                options.Value.ShellsContainerName);

            Directory.CreateDirectory(_tenantsContainerPath);

            var environmentName = hostingEnvironment.EnvironmentName.ToUpperInvariant();
            var applicationName = hostingEnvironment.ApplicationName.Replace('.', '_').ToUpperInvariant();
            var appsettingsPath = Path.Combine(options.Value.ShellsApplicationDataPath, "appsettings");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)

                .AddEnvironmentVariables($"{applicationName}_SETTINGS_")
                .AddEnvironmentVariables($"{applicationName}_{environmentName}_SETTINGS_")

                .AddJsonFile($"{appsettingsPath}.json", optional: true)
                .AddJsonFile($"{appsettingsPath}.{hostingEnvironment.EnvironmentName}.json", optional: true);

            var configurationProviders = new List<IConfigurationProvider>();

            foreach (var source in configurationSources)
            {
                var provider = source.Build(configurationBuilder);
                configurationProviders.Add(provider);
            }

            var commandLineProvider = (applicationConfiguration as IConfigurationRoot)?
                .Providers.FirstOrDefault(p => p is CommandLineConfigurationProvider);

            if (commandLineProvider != null)
            {
                configurationProviders.Add(commandLineProvider);
            }

            configurationBuilder.AddConfiguration(new ConfigurationRoot(configurationProviders));

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
                // The file may be own by another process or already exists when trying to create a new one.
            }
        }
    }
}