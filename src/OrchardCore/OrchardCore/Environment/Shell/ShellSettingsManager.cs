using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly string _tenantsContainerPath;
        private readonly IEnumerable<string> _configuredTenantFolders;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            _tenantsContainerPath = Path.Combine(
                options.Value.ShellsApplicationDataPath,
                options.Value.ShellsContainerName);

            Directory.CreateDirectory(_tenantsContainerPath);

            var environment = hostingEnvironment.EnvironmentName.ToUpperInvariant();
            var ENVIRONMENT = hostingEnvironment.EnvironmentName.ToUpperInvariant();
            var appsettings = Path.Combine(options.Value.ShellsApplicationDataPath, "appsettings");

            var Configuration = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)

                .AddJsonFile($"{appsettings}.json", optional: true)
                .AddJsonFile($"{appsettings}.{environment}.json", optional: true)

                .AddEnvironmentVariables("ORCHARDCORE_SETTINGS_")
                .AddEnvironmentVariables($"ORCHARDCORE_{ENVIRONMENT}_SETTINGS_")
                .Build();

            _configuredTenantFolders = Configuration.GetSection("Tenants").GetChildren()
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => Path.Combine(_tenantsContainerPath, section.Key))
                .Distinct()
                .ToArray();
        }

        public IConfiguration Configuration { get; }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            // Add the folders of pre-configured tenants to the existing ones.
            var tenantFolders = Directory.GetDirectories(_tenantsContainerPath)
                .Concat(_configuredTenantFolders).Distinct();

            var shellSettings = new ConcurrentBag<ShellSettings>();

            // Load all configuration in parallel
            Parallel.ForEach(tenantFolders, new ParallelOptions { MaxDegreeOfParallelism = 8 },
                (tenantFolder) =>
            {
                var tenantName = Path.GetFileName(tenantFolder);

                var localConfiguration = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(tenantFolder, "appsettings.json"), optional: true)
                    .Build();

                var shellSetting = new ShellSettings() { Name = tenantName };

                Configuration.Bind("Tenants", shellSetting);
                Configuration.GetSection("Tenants").Bind(tenantName, shellSetting);
                localConfiguration.Bind(shellSetting);

                shellSettings.Add(shellSetting);
            });

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

            Configuration.Bind("Tenants", globalSettings);
            Configuration.GetSection("Tenants").Bind(settings.Name, globalSettings);

            var localObject = JObject.FromObject(settings);
            var globalObject = JObject.FromObject(globalSettings);

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
                File.WriteAllText(Path.Combine(tenantFolder, "appsettings.json"), localObject.ToString());
            }

            catch (IOException)
            {
                // The file may be own by another process or already exists when trying to create a new one.
            }
        }
    }
}