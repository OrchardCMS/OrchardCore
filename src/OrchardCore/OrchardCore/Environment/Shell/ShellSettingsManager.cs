using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IEnumerable<string> _configuredTenants;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            var applicationDataPath = options.Value.ShellsApplicationDataPath;

            _tenantsContainerPath = Path.Combine(applicationDataPath, options.Value.ShellsContainerName);

            Directory.CreateDirectory(_tenantsContainerPath);

            var environment = hostingEnvironment.EnvironmentName.ToUpperInvariant();
            var ENVIRONMENT = hostingEnvironment.EnvironmentName.ToUpperInvariant();
            var appsettings = Path.Combine(applicationDataPath, "appsettings");

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)

                .AddJsonFile($"{appsettings}.json", optional: true)
                .AddJsonFile($"{appsettings}.{environment}.json", optional: true)

                .AddEnvironmentVariables("ORCHARDCORE_SETTINGS_")
                .AddEnvironmentVariables($"ORCHARDCORE_{ENVIRONMENT}_SETTINGS_");

            var commandLineProvider = (applicationConfiguration as IConfigurationRoot)?
                .Providers.FirstOrDefault(p => p is CommandLineConfigurationProvider);

            if (commandLineProvider != null)
            {
                configurationBuilder.AddConfiguration(new ConfigurationRoot(new[] { commandLineProvider }));
            }

            Configuration = configurationBuilder.Build();

            _configuredTenants = Configuration.GetSection("Tenants").GetChildren()
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => section.Key)
                .Distinct()
                .ToArray();
        }

        public IConfiguration Configuration { get; }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var localConfiguration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(_tenantsContainerPath, "tenants.json"), optional: true)
                .Build();

            var localTenants = localConfiguration.GetChildren().Select(section => section.Key);
            var tenants = _configuredTenants.Concat(localTenants).Distinct().ToArray();

            var shellSettings = new ConcurrentBag<ShellSettings>();

            Parallel.ForEach(tenants, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (tenant) =>
            {
                var shellSetting = new ShellSettings() { Name = tenant };

                Configuration.Bind("Tenants", shellSetting);
                Configuration.GetSection("Tenants").Bind(tenant, shellSetting);
                localConfiguration.Bind(tenant, shellSetting);

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

            lock (this)
            {
                try
                {
                    var localConfiguration = JObject.Parse(File.ReadAllText(Path.Combine(_tenantsContainerPath, "tenants.json")));

                    localConfiguration[settings.Name] = localObject;

                    File.WriteAllText(Path.Combine(_tenantsContainerPath, "tenants.json"), localConfiguration.ToString());
                }

                catch (IOException)
                {
                }
            }
        }
    }
}