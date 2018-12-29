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
        private readonly string _tenantsFilePath;
        private readonly string _tenantsContainerPath;
        private readonly IEnumerable<string> _configuredTenants;
        private readonly IConfiguration _configuration;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            var appDataPath = options.Value.ShellsApplicationDataPath;

            _tenantsContainerPath = Path.Combine(appDataPath, options.Value.ShellsContainerName);
            Directory.CreateDirectory(_tenantsContainerPath);

            _tenantsFilePath = Path.Combine(_tenantsContainerPath, "tenants.json");

            var environment = hostingEnvironment.EnvironmentName;
            var ENVIRONMENT = environment.ToUpperInvariant();
            var appsettings = Path.Combine(appDataPath, "appsettings");

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)

                .AddJsonFile($"{appsettings}.json", optional: true)
                .AddJsonFile($"{appsettings}.{environment}.json", optional: true)

                .AddEnvironmentVariables("ORCHARDCORE_")
                .AddEnvironmentVariables($"ORCHARDCORE_{ENVIRONMENT}_");

            var commandLineProvider = (applicationConfiguration as IConfigurationRoot)?
                .Providers.FirstOrDefault(p => p is CommandLineConfigurationProvider);

            if (commandLineProvider != null)
            {
                configurationBuilder.AddConfiguration(new ConfigurationRoot(new[] { commandLineProvider }));
            }

            _configuration = configurationBuilder.Build().GetSection("Tenants");

            _configuredTenants = _configuration.GetChildren()
                .Where(section => section["State"] != null)
                .Select(section => section.Key).Distinct()
                .ToArray();
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var tenantsSettings = new ConfigurationBuilder()
                .AddJsonFile(_tenantsFilePath, optional: true)
                .Build();

            var tenants = tenantsSettings.GetChildren().Select(section => section.Key);
            var allTenants = _configuredTenants.Concat(tenants).Distinct().ToArray();

            var allSettings = new ConcurrentBag<ShellSettings>();

            Parallel.ForEach(allTenants, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (tenant) =>
            {
                var localConfigPath = Path.Combine(_tenantsContainerPath, tenant, "appsettings.json");

                var configurationBuilder = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddJsonFile(localConfigPath, optional: true);

                var shellSettings = new ShellSettings(configurationBuilder)
                {
                    Name = tenant
                };

                _configuration.Bind(shellSettings);
                _configuration.Bind(tenant, shellSettings);
                tenantsSettings.Bind(tenant, shellSettings);

                allSettings.Add(shellSettings);
            });

            return allSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var shellSettings = new ShellSettings()
            {
                Name = settings.Name
            };

            _configuration.Bind(shellSettings);
            _configuration.Bind(settings.Name, shellSettings);

            var configObject = JObject.FromObject(shellSettings);
            var settingsObject = JObject.FromObject(settings);

            settingsObject.Remove("Name");

            foreach (var property in configObject)
            {
                var setting = settingsObject.Value<string>(property.Key);
                var configValue = configObject.Value<string>(property.Key);

                if (setting == null || setting == configValue)
                {
                    settingsObject.Remove(property.Key);
                }
            }

            lock (this)
            {
                try
                {
                    var tenantsObject = !File.Exists(_tenantsFilePath) ? new JObject()
                        : JObject.Parse(File.ReadAllText(_tenantsFilePath));

                    tenantsObject[settings.Name] = settingsObject;

                    File.WriteAllText(_tenantsFilePath, tenantsObject.ToString());
                }

                catch (IOException)
                {
                }
            }

            var configuration = new ConfigurationBuilder()
                .AddConfiguration(_configuration)
                .AddConfiguration(_configuration.GetSection(settings.Name))
                .Build();

            var localConfigObject = new JObject();

            foreach (var section in settings.Configuration.GetChildren().Where(s => s != null))
            {
                if (section.Value != configuration[section.Key])
                {
                    localConfigObject[section.Key] = section.Value;
                }
            }

            localConfigObject.Remove("Name");

            var tenantFolder = Path.Combine(_tenantsContainerPath, settings.Name);
            var localConfigPath = Path.Combine(tenantFolder, "appsettings.json");

            try
            {
                Directory.CreateDirectory(tenantFolder);
                File.WriteAllText(localConfigPath, localConfigObject.ToString());
            }

            catch (IOException)
            {
            }
        }
    }
}