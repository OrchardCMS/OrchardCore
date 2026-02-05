using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Settings.Services;

/// <summary>
/// Factory for creating configurable settings service instances that can be used from singleton contexts.
/// This is useful when settings need to be accessed from IConfigureOptions implementations.
/// </summary>
/// <typeparam name="TSettings">The type of settings.</typeparam>
public class ConfigurableSettingsServiceFactory<TSettings>
    where TSettings : class, IConfigurableSettings, new()
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ISettingsMergeStrategy<TSettings> _mergeStrategy;
    private readonly string _configurationKey;

    private TSettings _cachedFileSettings;
    private bool _fileSettingsLoaded;
    private readonly object _fileSettingsLock = new();

    public ConfigurableSettingsServiceFactory(
        IShellConfiguration shellConfiguration,
        ISettingsMergeStrategy<TSettings> mergeStrategy,
        string configurationKey)
    {
        _shellConfiguration = shellConfiguration;
        _mergeStrategy = mergeStrategy;
        _configurationKey = configurationKey;
    }

    /// <summary>
    /// Gets the file configuration settings (from appsettings.json).
    /// This is safe to call from singleton contexts.
    /// </summary>
    /// <returns>The settings from configuration files.</returns>
    public TSettings GetFileConfigurationSettings()
    {
        if (_fileSettingsLoaded)
        {
            return _cachedFileSettings;
        }

        lock (_fileSettingsLock)
        {
            if (_fileSettingsLoaded)
            {
                return _cachedFileSettings;
            }

            var settings = new TSettings();

            if (!string.IsNullOrEmpty(_configurationKey))
            {
                var section = _shellConfiguration.GetSection(_configurationKey);
                if (section.Exists())
                {
                    section.Bind(settings);
                }
            }

            _cachedFileSettings = settings;
            _fileSettingsLoaded = true;

            return settings;
        }
    }

    /// <summary>
    /// Merges the provided database settings with file configuration settings.
    /// </summary>
    /// <param name="databaseSettings">The settings from the database.</param>
    /// <param name="serviceProvider">Optional service provider for custom merge functions.</param>
    /// <returns>The merged effective settings.</returns>
    public TSettings MergeSettings(TSettings databaseSettings, IServiceProvider serviceProvider = null)
    {
        var fileSettings = GetFileConfigurationSettings();
        return _mergeStrategy.Merge(databaseSettings, fileSettings, serviceProvider);
    }

    /// <summary>
    /// Gets whether UI configuration is disabled via file configuration.
    /// </summary>
    public bool IsUIConfigurationDisabled
    {
        get
        {
            var fileSettings = GetFileConfigurationSettings();
            return fileSettings?.DisableUIConfiguration ?? false;
        }
    }
}
