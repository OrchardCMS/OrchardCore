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
                var localConfigurationPath = Path.Combine(_tenantsContainerPath, tenant, "appsettings.json");

                var configurationBuilder = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddJsonFile(localConfigurationPath, optional: true);

                var shellSettings = new ShellSettings()
                {
                    Name = tenant,
                    ConfigurationBuilder = configurationBuilder
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

            var shellSettings = new ShellSettings() { Name = settings.Name };

            _configuration.Bind(shellSettings);
            _configuration.Bind(settings.Name, shellSettings);

            var localSettings = JObject.FromObject(settings);
            var globalSettings = JObject.FromObject(shellSettings);

            localSettings.Remove("Name");

            foreach (var property in globalSettings)
            {
                var local = localSettings.Value<string>(property.Key);
                var global = globalSettings.Value<string>(property.Key);

                if (local == null || global == local)
                {
                    localSettings.Remove(property.Key);
                }
            }

            lock (this)
            {
                try
                {
                    JObject tenantsSettings;

                    if (File.Exists(_tenantsFilePath))
                    {
                        tenantsSettings = JObject.Parse(File.ReadAllText(_tenantsFilePath));
                    }
                    else
                    {
                        tenantsSettings = new JObject();
                    }

                    tenantsSettings[settings.Name] = localSettings;

                    File.WriteAllText(_tenantsFilePath, tenantsSettings.ToString());
                }

                catch (IOException)
                {
                }
            }

            var globalConfiguration = new ConfigurationBuilder()
                .AddConfiguration(_configuration)
                .AddConfiguration(_configuration.GetSection(settings.Name))
                .Build();

            var localConfiguration = new JObject();

            foreach (var section in settings.Configuration.GetChildren())
            {
                var local = section.Value;
                var global = globalConfiguration[section.Key];

                if (local != null && global != local)
                {
                    localConfiguration[section.Key] = section.Value;
                }
            }

            localSettings.Remove("Name");

            var tenantFolder = Path.Combine(_tenantsContainerPath, settings.Name);
            var localConfigurationPath = Path.Combine(tenantFolder, "appsettings.json");

            try
            {
                Directory.CreateDirectory(tenantFolder);
                File.WriteAllText(localConfigurationPath, localConfiguration.ToString());
            }

            catch (IOException)
            {
            }
        }
    }
}