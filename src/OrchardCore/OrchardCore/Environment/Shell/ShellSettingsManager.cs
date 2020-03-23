using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly IConfiguration _applicationConfiguration;
        private readonly IShellsConfigurationSources _tenantsConfigSources;
        private readonly IShellConfigurationSources _tenantConfigSources;
        private readonly IShellsSettingsSources _settingsSources;

        private IConfiguration _configuration;
        private IEnumerable<string> _configuredTenants;
        private Func<string, Task<IConfigurationBuilder>> _configBuilderFactory;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IShellsConfigurationSources tenantsConfigSources,
            IShellConfigurationSources tenantConfigSources,
            IShellsSettingsSources settingsSources)
        {
            _applicationConfiguration = applicationConfiguration;
            _tenantsConfigSources = tenantsConfigSources;
            _tenantConfigSources = tenantConfigSources;
            _settingsSources = settingsSources;
        }

        public ShellSettings CreateDefaultSettings()
        {
            return new ShellSettings
            (
                new ShellConfiguration(_configuration),
                new ShellConfiguration(_configuration)
            );
        }

        public async Task<IEnumerable<ShellSettings>> LoadSettingsAsync()
        {
            await EnsureConfigurationAsync();

            await _semaphore.WaitAsync();
            try
            {
                var tenantsSettings = (await new ConfigurationBuilder()
                    .AddSourcesAsync(_settingsSources))
                    .Build();

                var tenants = tenantsSettings.GetChildren().Select(section => section.Key);
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
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<ShellSettings> LoadSettingsAsync(string tenant)
        {
            await EnsureConfigurationAsync();

            await _semaphore.WaitAsync();
            try
            {
                var tenantsSettings = (await new ConfigurationBuilder()
                    .AddSourcesAsync(_settingsSources))
                    .Build();

                var tenantSettings = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddConfiguration(tenantsSettings.GetSection(tenant))
                    .Build();

                var settings = new ShellConfiguration(tenantSettings);
                var configuration = new ShellConfiguration(tenant, _configBuilderFactory);

                return new ShellSettings(settings, configuration)
                {
                    Name = tenant,
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveSettingsAsync(ShellSettings settings)
        {
            await EnsureConfigurationAsync();

            await _semaphore.WaitAsync();
            try
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

                foreach (var property in configSettings)
                {
                    var tenantValue = tenantSettings.Value<string>(property.Key);
                    var configValue = configSettings.Value<string>(property.Key);

                    if (tenantValue != configValue)
                    {
                        tenantSettings[property.Key] = tenantValue;
                    }
                    else
                    {
                        tenantSettings[property.Key] = null;
                    }
                }

                tenantSettings.Remove("Name");

                await _settingsSources.SaveAsync(settings.Name, tenantSettings.ToObject<Dictionary<string, string>>());

                var tenantConfig = new JObject();

                var sections = settings.ShellConfiguration.GetChildren()
                    .Where(s => !s.GetChildren().Any())
                    .ToArray();

                foreach (var section in sections)
                {
                    if (settings[section.Key] != configuration[section.Key])
                    {
                        tenantConfig[section.Key] = settings[section.Key];
                    }
                    else
                    {
                        tenantConfig[section.Key] = null;
                    }
                }

                tenantConfig.Remove("Name");

                await _tenantConfigSources.SaveAsync(settings.Name, tenantConfig.ToObject<Dictionary<string, string>>());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task EnsureConfigurationAsync()
        {
            if (_configuration != null)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_configuration != null)
                {
                    return;
                }

                var lastProviders = (_applicationConfiguration as IConfigurationRoot)?.Providers
                    .Where(p => p is EnvironmentVariablesConfigurationProvider ||
                                p is CommandLineConfigurationProvider)
                    .ToArray();

                var configurationBuilder = await new ConfigurationBuilder()
                    .AddConfiguration(_applicationConfiguration)
                    .AddSourcesAsync(_tenantsConfigSources);

                if (lastProviders.Count() > 0)
                {
                    configurationBuilder.AddConfiguration(new ConfigurationRoot(lastProviders));
                }

                var configuration = configurationBuilder.Build().GetSection("OrchardCore");

                _configuredTenants = configuration.GetChildren()
                    .Where(section => Enum.TryParse<TenantState>(section["State"], ignoreCase: true, out var result))
                    .Select(section => section.Key)
                    .Distinct()
                    .ToArray();

                _configBuilderFactory = async (tenant) =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        var builder = new ConfigurationBuilder().AddConfiguration(_configuration);
                        builder.AddConfiguration(configuration.GetSection(tenant));
                        return await builder.AddSourcesAsync(tenant, _tenantConfigSources);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                };

                _configuration = configuration;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
