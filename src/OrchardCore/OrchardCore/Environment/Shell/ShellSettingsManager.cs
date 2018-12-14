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

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddEnvironmentVariables("TENANTS_SETTINGS_")
                .AddEnvironmentVariables($"TENANTS_{hostingEnvironment.EnvironmentName.ToUpperInvariant()}_SETTINGS_")
                .AddJsonFile("tenants.settings.json", optional: true)
                .AddJsonFile($"tenants.{hostingEnvironment.EnvironmentName}.settings.json", optional: true);

            foreach (var configurationProvider in configurationProviders.OrderBy(p => p.Order))
            {
                configurationBuilder.AddConfiguration(configurationProvider.Configuration);
            }

            Configuration = configurationBuilder.Build();
        }

        public IConfiguration Configuration { get; }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var configuredTenantFolders = Configuration.GetChildren()
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => Path.Combine(_tenantsContainerPath, section.Key));

            var tenantFolders = Directory.GetDirectories(_tenantsContainerPath)
                .Concat(configuredTenantFolders).Distinct();

            var shellSettings = new List<ShellSettings>();

            foreach (var tenantFolder in tenantFolders)
            {
                var tenantName = Path.GetFileName(tenantFolder);

                var localConfiguration = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(tenantFolder, "settings.json"), optional: true)
                    .Build();

                var shellSetting = new ShellSettings() { Name = tenantName };

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

            Configuration.Bind(globalSettings);
            Configuration.Bind(settings.Name, globalSettings);

            var globalObject = JObject.FromObject(globalSettings);
            var localObject = JObject.FromObject(settings);

            foreach (var property in globalObject)
            {
                if (property.Key != "Name")
                {
                    var localValue = localObject.Value<string>(property.Key);
                    var globalValue = globalObject.Value<string>(property.Key);

                    if (localValue == null || globalValue == localValue)
                    {
                        localObject.Remove(property.Key);
                    }
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(tenantFolder, "settings.json"), localObject.ToString());
            }

            catch (IOException) { }
        }
    }
}