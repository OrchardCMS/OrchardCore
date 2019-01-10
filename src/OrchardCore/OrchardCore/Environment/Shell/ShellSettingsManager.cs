using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly string _tenantsContainerPath;
        private readonly string _tenantsFilePath;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<string> _configuredTenants;
        private readonly Func<string, IConfigurationBuilder> _configBuilderFactory;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> options)
        {
            // e.g., App_Data
            var appDataPath = options.Value.ShellsApplicationDataPath;

            // e.g., App_Data/Sites
            _tenantsContainerPath = Path.Combine(appDataPath, options.Value.ShellsContainerName);
            Directory.CreateDirectory(_tenantsContainerPath);

            _tenantsFilePath = Path.Combine(appDataPath, "tenants.json");

            var environment = hostingEnvironment.EnvironmentName;
            var appsettings = Path.Combine(appDataPath, "appsettings");

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)

                .AddJsonFile($"{appsettings}.json", optional: true)
                .AddJsonFile($"{appsettings}.{environment}.json", optional: true);

            var commandLineProvider = (applicationConfiguration as IConfigurationRoot)?
                .Providers.FirstOrDefault(p => p is CommandLineConfigurationProvider);

            if (commandLineProvider != null)
            {
                configurationBuilder.AddConfiguration(new ConfigurationRoot(new[] { commandLineProvider }));
            }

            _configuration = configurationBuilder.Build().GetSection("OrchardCore");

            _configuredTenants = _configuration.GetChildren()
                .Where(section => section["State"] != null)
                .Select(section => section.Key)
                .Distinct()
                .ToArray();

            _configBuilderFactory = (tenant) => new ConfigurationBuilder()
                .AddConfiguration(_configuration)
                .AddConfiguration(_configuration.GetSection(tenant))
                .AddJsonFile(Path.Combine(_tenantsContainerPath, tenant, "appsettings.json"), optional: true);
        }

        public ShellSettings CreateDefaultSettings()
        {
            Func<string, IConfigurationBuilder> factory = (tenant) => new ConfigurationBuilder()
                .AddConfiguration(_configuration);

            var shellConfiguration = new ShellConfiguration(null, factory);

            return new ShellSettings(shellConfiguration)
            {
                RequestUrlHost = shellConfiguration["RequestUrlHost"],
                RequestUrlPrefix = shellConfiguration["RequestUrlPrefix"],
                State = TenantState.Uninitialized
            };
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var tenantsSettings = new ConfigurationBuilder()
                .AddJsonFile(_tenantsFilePath, optional: true)
                .Build();

            var tenants = tenantsSettings.GetChildren().Select(section => section.Key);
            var allTenants = _configuredTenants.Concat(tenants).Distinct().ToArray();

            var allSettings = new List<ShellSettings>();

            foreach (var tenant in allTenants)
            {
                var settings = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddConfiguration(tenantsSettings.GetSection(tenant))
                    .Build();

                var shellConfiguration = new ShellConfiguration(tenant, _configBuilderFactory);

                var shellSettings = new ShellSettings(shellConfiguration)
                {
                    Name = tenant,
                    RequestUrlHost = settings["RequestUrlHost"],
                    RequestUrlPrefix = settings["RequestUrlPrefix"],
                    State = settings.GetValue<TenantState>("State")
                };

                allSettings.Add(shellSettings);
            };

            return allSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            lock (this)
            {
                if (settings == null)
                {
                    throw new ArgumentNullException(nameof(settings));
                }

                var configuration = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(settings.Name))
                    .Build();

                var shellSettings = new ShellSettings()
                {
                    Name = settings.Name
                };

                configuration.Bind(shellSettings);

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

                var tenantsObject = !File.Exists(_tenantsFilePath) ? new JObject()
                    : JObject.Parse(File.ReadAllText(_tenantsFilePath));

                tenantsObject[settings.Name] = settingsObject;
                File.WriteAllText(_tenantsFilePath, tenantsObject.ToString());

                var localConfigObject = new JObject();

                var sections = settings.ShellConfiguration.GetChildren().Where(s => s.Value != null).ToArray();

                foreach (var section in sections)
                {
                    if (section.Value != configuration[section.Key])
                    {
                        localConfigObject[section.Key] = section.Value;
                    }
                }

                localConfigObject.Remove("Name");

                var tenantFolder = Path.Combine(_tenantsContainerPath, settings.Name);
                var localConfigPath = Path.Combine(tenantFolder, "appsettings.json");

                Directory.CreateDirectory(tenantFolder);
                File.WriteAllText(localConfigPath, localConfigObject.ToString());
            }
        }
    }
}