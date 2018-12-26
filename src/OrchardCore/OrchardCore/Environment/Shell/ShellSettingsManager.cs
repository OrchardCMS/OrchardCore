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
                .Where(section => section.GetValue<string>("State") != null)
                .Select(section => section.Key).Distinct().ToArray();
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var tenantsConfiguration = new ConfigurationBuilder()
                .AddJsonFile(_tenantsFilePath, optional: true)
                .Build();

            var localTenants = tenantsConfiguration.GetChildren().Select(section => section.Key);
            var tenants = _configuredTenants.Concat(localTenants).Distinct().ToArray();

            var shellsSettings = new ConcurrentBag<ShellSettings>();

            Parallel.ForEach(tenants, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (tenant) =>
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
                tenantsConfiguration.Bind(tenant, shellSettings);

                shellsSettings.Add(shellSettings);
            });

            return shellsSettings;
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

            var localObject = JObject.FromObject(settings);
            var globalObject = JObject.FromObject(shellSettings);

            localObject.Remove("Name");

            foreach (var property in globalObject)
            {
                var localValue = localObject.Value<string>(property.Key);
                var globalValue = globalObject.Value<string>(property.Key);

                if (localValue == null || globalValue == localValue)
                {
                    localObject.Remove(property.Key);
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

                    tenantsSettings[settings.Name] = localObject;

                    File.WriteAllText(_tenantsFilePath, tenantsSettings.ToString());
                }

                catch (IOException)
                {
                }
            }

            var globalData = new ConfigurationBuilder()
                .AddConfiguration(_configuration)
                .AddConfiguration(_configuration.GetSection(settings.Name))
                .Build().GetChildren()
                .Where(s => s.Value != null)
                .ToDictionary(s => s.Key, s => s.Value);

            var localData = settings.Configuration.GetChildren()
                .Where(s => s.Value != null)
                .ToDictionary(s => s.Key, s => s.Value);

            localObject = new JObject();

            foreach (var local in localData)
            {
                if (!globalData.TryGetValue(local.Key, out var globalValue) ||
                    local.Value != globalValue)
                {
                    localObject[local.Key] = local.Value;
                }
            }

            var tenantFolder = Path.Combine(_tenantsContainerPath, settings.Name);
            var localConfigurationPath = Path.Combine(tenantFolder, "appsettings.json");

            try
            {
                Directory.CreateDirectory(tenantFolder);
                File.WriteAllText(localConfigurationPath, localObject.ToString());
            }

            catch (IOException)
            {
            }
        }
    }
}