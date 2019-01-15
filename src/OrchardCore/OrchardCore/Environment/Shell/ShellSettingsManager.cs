using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Models;
using YamlDotNet.Serialization;

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

            var appsettings = Path.Combine(appDataPath, "appsettings");
            var environment = hostingEnvironment.EnvironmentName;

            var lastProviders = (applicationConfiguration as IConfigurationRoot)?.Providers
                .Where(p => p is EnvironmentVariablesConfigurationProvider ||
                            p is CommandLineConfigurationProvider)
                .ToList();

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)
                .AddJsonFile($"{appsettings}.json", optional: true)
                .AddJsonFile($"{appsettings}.{environment}.json", optional: true);

            if (lastProviders.Count() > 0)
            {
                configurationBuilder.AddConfiguration(new ConfigurationRoot(lastProviders));
            }

            _configuration = configurationBuilder.Build().GetSection("OrchardCore");

            _configuredTenants = _configuration.GetChildren()
                .Where(section => section["State"] != null)
                .Select(section => section.Key)
                .Distinct()
                .ToArray();

            _configBuilderFactory = (tenant) =>
            {
                var builder = new ConfigurationBuilder().AddConfiguration(_configuration);

                if (_configuredTenants.Contains(tenant))
                {
                    builder.AddConfiguration(_configuration.GetSection(tenant));
                }

                return builder.AddJsonFile(Path.Combine(_tenantsContainerPath, tenant, "appsettings.json"), optional: true);
            };
        }

        public ShellSettings CreateDefaultSettings()
        {
            Func<string, IConfigurationBuilder> factory = (tenant) =>
                new ConfigurationBuilder().AddConfiguration(_configuration);

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
            if (!File.Exists(_tenantsFilePath) && File.Exists(Path.Combine(_tenantsContainerPath,
                ShellHelper.DefaultShellName, "Settings.txt")))
            {
                // If no 'tenants.json' and an old 'Settings.txt', try to update from Beta2.
                UpgradeFromBeta2();
            }

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

                var tenantFolder = Path.Combine(_tenantsContainerPath, settings.Name);
                var localConfigPath = Path.Combine(tenantFolder, "appsettings.json");

                var localConfigObject = !File.Exists(localConfigPath) ? new JObject()
                    : JObject.Parse(File.ReadAllText(localConfigPath));

                var sections = settings.ShellConfiguration.GetChildren().Where(s => s.Value != null).ToArray();

                foreach (var section in sections)
                {
                    if (section.Value != configuration[section.Key])
                    {
                        localConfigObject[section.Key] = section.Value;
                    }
                }

                localConfigObject.Remove("Name");

                Directory.CreateDirectory(tenantFolder);
                File.WriteAllText(localConfigPath, localConfigObject.ToString());
            }
        }

        private void UpgradeFromBeta2()
        {
            // TODO: Can be removed when going RC as users are not supposed to go from beta2 to RC

            var tenantFolders = Directory.GetDirectories(_tenantsContainerPath);

            foreach (var tenantFolder in tenantFolders)
            {
                var oldSettingsPath = Path.Combine(tenantFolder, "Settings.txt");
                var localConfigPath = Path.Combine(tenantFolder, "appsettings.json");

                if (!File.Exists(oldSettingsPath) || File.Exists(localConfigPath))
                {
                    continue;
                }

                var tenant = Path.GetFileName(tenantFolder);
                var defaultSettings = CreateDefaultSettings();

                using (var reader = new StreamReader(oldSettingsPath))
                {
                    var yamlObject = new Deserializer().Deserialize(reader);
                    var settingsObject = JObject.FromObject(yamlObject)[tenant];

                    var shellSettings = new ShellSettings(defaultSettings)
                    {
                        Name = tenant,
                        RequestUrlHost = settingsObject.Value<string>("RequestUrlHost"),
                        RequestUrlPrefix = settingsObject.Value<string>("RequestUrlPrefix"),
                        State = Enum.TryParse<TenantState>(settingsObject.Value<string>("State"),
                            out var tenantState) ? tenantState : TenantState.Invalid
                    };

                    shellSettings["TablePrefix"] = settingsObject.Value<string>("TablePrefix");
                    shellSettings["DatabaseProvider"] = settingsObject.Value<string>("DatabaseProvider");
                    shellSettings["ConnectionString"] = settingsObject.Value<string>("ConnectionString");
                    shellSettings["RecipeName"] = settingsObject.Value<string>("RecipeName");
                    shellSettings["Secret"] = settingsObject.Value<string>("Secret");

                    SaveSettings(shellSettings);
                }
            }
        }
    }
}