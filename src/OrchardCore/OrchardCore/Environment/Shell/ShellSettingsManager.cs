using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Models;
using YamlDotNet.Serialization;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        // TODO: Can be removed when going to RC.
        private readonly string _tenantsContainerPath;

        private readonly IConfiguration _configuration;
        private readonly IEnumerable<string> _configuredTenants;
        private readonly Func<string, IConfigurationBuilder> _configBuilderFactory;
        private readonly ITenantConfigurationSources _tenantConfigSources;
        private readonly ITenantsSettingsSources _settingsSources;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            ITenantsConfigurationSources configurationSources,
            ITenantConfigurationSources tenantConfigSources,
            ITenantsSettingsSources settingsSources,
            IOptions<ShellOptions> options)
        {
            // TODO: Can be removed when going to RC.
            var appDataPath = options.Value.ShellsApplicationDataPath;
            _tenantsContainerPath = Path.Combine(appDataPath, options.Value.ShellsContainerName);

            _tenantConfigSources = tenantConfigSources;
            _settingsSources = settingsSources;

            var lastProviders = (applicationConfiguration as IConfigurationRoot)?.Providers
                .Where(p => p is EnvironmentVariablesConfigurationProvider ||
                            p is CommandLineConfigurationProvider)
                .ToArray();

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)
                .AddSources(configurationSources);

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

                return builder.AddSources(tenant, _tenantConfigSources);
            };
        }

        public ShellSettings CreateDefaultSettings()
        {
            Func<string, IConfigurationBuilder> factory = (tenant) =>
                new ConfigurationBuilder().AddConfiguration(_configuration);

            var settings = new ShellConfiguration(_configuration);
            var configuration = new ShellConfiguration(null, factory);

            return new ShellSettings(settings, configuration);
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var tenantsSettings = new ConfigurationBuilder()
                .AddSources(_settingsSources)
                .Build();

            var tenants = tenantsSettings.GetChildren().Select(section => section.Key);

            // TODO: Can be removed when going to RC.
            if (!tenants.Any() && File.Exists(Path.Combine(_tenantsContainerPath,
                ShellHelper.DefaultShellName, "Settings.txt")))
            {
                // If no tenants and an old 'Settings.txt', try to update from Beta2.
                UpgradeFromBeta2();
                tenantsSettings.Reload();
                tenants = tenantsSettings.GetChildren().Select(section => section.Key);
            }

            var allTenants = _configuredTenants.Concat(tenants).Distinct().ToArray();

            var allSettings = new List<ShellSettings>();

            foreach (var tenant in allTenants)
            {
                var tenantSettings = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddConfiguration(tenantsSettings.GetSection(tenant))
                    .Build();

                var settings = new ShellConfiguration(tenantSettings);
                var configuration = new ShellConfiguration(tenant, _configBuilderFactory);

                var shellSettings = new ShellSettings(settings, configuration)
                {
                    Name = tenant,
                };

                allSettings.Add(shellSettings);
            };

            return allSettings;
        }

        public void SaveSettings(ShellSettings settings)
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

            var configSettings = JObject.FromObject(shellSettings);
            var tenantSettings = JObject.FromObject(settings);

            tenantSettings.Remove("Name");

            foreach (var property in configSettings)
            {
                var tenantValue = tenantSettings.Value<string>(property.Key);
                var configValue = configSettings.Value<string>(property.Key);

                if (tenantValue == null || tenantValue == configValue)
                {
                    tenantSettings.Remove(property.Key);
                }
            }

            _settingsSources.Save(settings.Name, tenantSettings.ToObject<Dictionary<string, string>>());

            var tenantConfig = new JObject();

            var sections = settings.ShellConfiguration.GetChildren()
                .Where(s => s.Value != null)
                .ToArray();

            foreach (var section in sections)
            {
                if (section.Value != configuration[section.Key])
                {
                    tenantConfig[section.Key] = section.Value;
                }
                else
                {
                    tenantConfig[section.Key] = null;
                }
            }

            tenantConfig.Remove("Name");

            _tenantConfigSources.Save(settings.Name, tenantConfig.ToObject<Dictionary<string, string>>());
        }

        // TODO: Can be removed when going to RC.
        private void UpgradeFromBeta2()
        {
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