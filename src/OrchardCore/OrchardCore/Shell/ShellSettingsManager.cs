using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell;

public class ShellSettingsManager : IShellSettingsManager, IDisposable
{
    private readonly IConfiguration _applicationConfiguration;
    private readonly IShellsConfigurationSources _tenantsConfigSources;
    private readonly IShellConfigurationSources _tenantConfigSources;
    private readonly IShellsSettingsSources _tenantsSettingsSources;

    private IConfiguration _configuration;
    private IConfigurationRoot _configurationRoot;
    private IConfigurationRoot _tenantsSettingsRoot;
    private IEnumerable<string> _configuredTenants;
    private readonly SemaphoreSlim _semaphore = new(1);

    private Func<string, Action<IConfigurationBuilder>, Task<IConfigurationRoot>> _tenantConfigFactoryAsync;
    private readonly SemaphoreSlim _tenantConfigSemaphore = new(1);
    private bool _disposed;

    public ShellSettingsManager(
        IConfiguration applicationConfiguration,
        IShellsConfigurationSources tenantsConfigSources,
        IShellConfigurationSources tenantConfigSources,
        IShellsSettingsSources tenantsSettingsSources)
    {
        _applicationConfiguration = applicationConfiguration;
        _tenantsConfigSources = tenantsConfigSources;
        _tenantConfigSources = tenantConfigSources;
        _tenantsSettingsSources = tenantsSettingsSources;
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
        await _semaphore.WaitAsync();
        try
        {
            await EnsureConfigurationAsync();
            await ReloadTenantsSettingsAsync();

            var tenants = _tenantsSettingsRoot.GetChildren().Select(section => section.Key);
            var allTenants = _configuredTenants.Concat(tenants).Distinct().ToArray();

            var allSettings = new List<ShellSettings>();

            foreach (var tenant in allTenants)
            {
                var tenantSettingsBuilder = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddConfiguration(_tenantsSettingsRoot.GetSection(tenant));

                var settings = new ShellConfiguration(tenantSettingsBuilder);
                var configuration = new ShellConfiguration(tenant, _tenantConfigFactoryAsync);

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

    public async Task<IEnumerable<string>> LoadSettingsNamesAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            await EnsureConfigurationAsync();
            await ReloadTenantsSettingsAsync();

            var tenants = _tenantsSettingsRoot.GetChildren().Select(section => section.Key);
            return _configuredTenants.Concat(tenants).Distinct().ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ShellSettings> LoadSettingsAsync(string tenant)
    {
        await _semaphore.WaitAsync();
        try
        {
            await EnsureConfigurationAsync();
            await ReloadTenantsSettingsAsync();

            var tenantSettingsBuilder = new ConfigurationBuilder()
                .AddConfiguration(_configuration)
                .AddConfiguration(_configuration.GetSection(tenant))
                .AddConfiguration(_tenantsSettingsRoot.GetSection(tenant));

            var settings = new ShellConfiguration(tenantSettingsBuilder);
            var configuration = new ShellConfiguration(tenant, _tenantConfigFactoryAsync);

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
        await _semaphore.WaitAsync();
        try
        {
            await EnsureConfigurationAsync();

            ArgumentNullException.ThrowIfNull(settings);

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

            await _tenantsSettingsSources.SaveAsync(settings.Name, tenantSettings.ToObject<Dictionary<string, string>>());

            var tenantConfig = new Dictionary<string, string>();
            foreach (var config in settings.ShellConfiguration.AsEnumerable())
            {
                if (settings.ShellConfiguration.GetSection(config.Key).GetChildren().Any())
                {
                    continue;
                }

                if (settings[config.Key] != configuration[config.Key])
                {
                    tenantConfig[config.Key] = settings[config.Key];
                }
                else
                {
                    tenantConfig[config.Key] = null;
                }
            }

            tenantConfig.Remove("Name");

            await _tenantConfigSemaphore.WaitAsync();
            try
            {
                await _tenantConfigSources.SaveAsync(settings.Name, tenantConfig);
            }
            finally
            {
                _tenantConfigSemaphore.Release();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RemoveSettingsAsync(ShellSettings settings)
    {
        await _semaphore.WaitAsync();
        try
        {
            await EnsureConfigurationAsync();

            ArgumentNullException.ThrowIfNull(settings);

            await _tenantsSettingsSources.RemoveAsync(settings.Name);

            await _tenantConfigSemaphore.WaitAsync();
            try
            {
                await _tenantConfigSources.RemoveAsync(settings.Name);
            }
            finally
            {
                _tenantConfigSemaphore.Release();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task EnsureConfigurationAsync()
    {
        if (_configuration is not null)
        {
            return;
        }

        var lastProviders = (_applicationConfiguration as IConfigurationRoot)?.Providers
            .Where(p => p is EnvironmentVariablesConfigurationProvider ||
                        p is CommandLineConfigurationProvider)
            .ToArray()
            ?? [];

        var configurationBuilder = await new ConfigurationBuilder()
            .AddConfiguration(_applicationConfiguration)
            .AddSourcesAsync(_tenantsConfigSources);

        if (lastProviders.Length > 0)
        {
            configurationBuilder.AddConfiguration(new ConfigurationRoot(lastProviders));
        }

        _configurationRoot = configurationBuilder.Build();
        var configuration = _configurationRoot.GetSection("OrchardCore");

        _configuredTenants = configuration.GetChildren()
            .Where(section => Enum.TryParse<TenantState>(section["State"], ignoreCase: true, out _))
            .Select(section => section.Key)
            .Distinct()
            .ToArray();

        _tenantConfigFactoryAsync = async (tenant, configure) =>
        {
            await _tenantConfigSemaphore.WaitAsync();
            try
            {
                var builder = await new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddSourcesAsync(tenant, _tenantConfigSources);

                configure(builder);

                return builder.Build();
            }
            finally
            {
                _tenantConfigSemaphore.Release();
            }
        };

        _configuration = configuration;
    }

    private async Task ReloadTenantsSettingsAsync()
    {
        using var disposable = _tenantsSettingsRoot as IDisposable;

        _tenantsSettingsRoot = (await new ConfigurationBuilder()
            .AddSourcesAsync(_tenantsSettingsSources))
            .Build();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        (_configurationRoot as IDisposable)?.Dispose();
        (_tenantsSettingsRoot as IDisposable)?.Dispose();

        _semaphore?.Dispose();
        _tenantConfigSemaphore?.Dispose();

        GC.SuppressFinalize(this);
    }
}
