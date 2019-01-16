using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // TODO: Can be removed when going to RC.
        private readonly string _tenantsContainerPath;
        private readonly string _tenantsFilePath;

        private readonly IConfiguration _configuration;
        private readonly IEnumerable<string> _configuredTenants;
        private readonly Func<string, IConfigurationBuilder> _configBuilderFactory;
        private readonly IEnumerable<IShellConfigurationSources> _shellConfigurationSources;
        private readonly IShellSettingsSources _shellSettingsSources;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IEnumerable<IShellConfigurationSources> shellConfigurationSources,
            IShellSettingsSources shellSettingsSources,
            IOptions<ShellOptions> options)
        {
            // TODO: Can be removed when going to RC.
            var appDataPath = options.Value.ShellsApplicationDataPath;
            _tenantsContainerPath = Path.Combine(appDataPath, options.Value.ShellsContainerName);
            _tenantsFilePath = Path.Combine(appDataPath, "tenants.json");

            _shellConfigurationSources = shellConfigurationSources;
            _shellSettingsSources = shellSettingsSources;

            var lastProviders = (applicationConfiguration as IConfigurationRoot)?.Providers
                .Where(p => p is EnvironmentVariablesConfigurationProvider ||
                            p is CommandLineConfigurationProvider)
                .ToArray();

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration);

            foreach (var sources in _shellConfigurationSources)
            {
                configurationBuilder.AddSources(sources);
            }

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

                var sources = _shellConfigurationSources.LastOrDefault();

                return builder.AddSources(tenant, sources);
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
            // TODO: Can be removed when going to RC.
            if (!File.Exists(_tenantsFilePath) && File.Exists(Path.Combine(_tenantsContainerPath,
                ShellHelper.DefaultShellName, "Settings.txt")))
            {
                // If no 'tenants.json' and an old 'Settings.txt', try to update from Beta2.
                UpgradeFromBeta2();
            }

            var tenantsSettings = new ConfigurationBuilder()
                .AddSources(_shellSettingsSources)
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
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var baseConfig = new ConfigurationBuilder()
                .AddConfiguration(_configuration)
                .AddConfiguration(_configuration.GetSection(settings.Name))
                .Build();

            var shellSettings = new ShellSettings()
            {
                Name = settings.Name
            };

            baseConfig.Bind(shellSettings);

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

            _shellSettingsSources.Save(settings.Name,
                settingsObject.ToObject<Dictionary<string, string>>());

            var localConfig = new JObject();

            var sections = settings.ShellConfiguration.GetChildren()
                .Where(s => s.Value != null)
                .ToArray();

            foreach (var section in sections)
            {
                if (section.Value != baseConfig[section.Key])
                {
                    localConfig[section.Key] = section.Value;
                }
                else
                {
                    localConfig[section.Key] = null;
                }
            }

            localConfig.Remove("Name");

            _shellConfigurationSources.LastOrDefault().Save(settings.Name,
                localConfig.ToObject<Dictionary<string, string>>());
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