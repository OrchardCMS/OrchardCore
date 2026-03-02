using System.Reflection;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Settings.Services;

/// <summary>
/// Default implementation of <see cref="IConfigurableSettingsService{TSettings}"/>.
/// </summary>
/// <typeparam name="TSettings">The type of settings to manage.</typeparam>
public class ConfigurableSettingsService<TSettings> : IConfigurableSettingsService<TSettings>
    where TSettings : class, IConfigurableSettings, new()
{
    private readonly ISiteService _siteService;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ISettingsMergeStrategy<TSettings> _mergeStrategy;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _configurationKey;

    private TSettings _cachedFileSettings;
    private bool _fileSettingsLoaded;
    private readonly object _fileSettingsLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurableSettingsService{TSettings}"/> class.
    /// </summary>
    /// <param name="siteService">The site service for accessing database settings.</param>
    /// <param name="shellConfiguration">The shell configuration for accessing file settings.</param>
    /// <param name="mergeStrategy">The merge strategy to use.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="configurationKey">The configuration key for binding file settings.</param>
    public ConfigurableSettingsService(
        ISiteService siteService,
        IShellConfiguration shellConfiguration,
        ISettingsMergeStrategy<TSettings> mergeStrategy,
        IServiceProvider serviceProvider,
        string configurationKey)
    {
        _siteService = siteService;
        _shellConfiguration = shellConfiguration;
        _mergeStrategy = mergeStrategy;
        _serviceProvider = serviceProvider;
        _configurationKey = configurationKey;
    }

    /// <inheritdoc/>
    public async Task<TSettings> GetEffectiveSettingsAsync()
    {
        var databaseSettings = await GetDatabaseSettingsAsync();
        var fileSettings = GetFileConfigurationSettings();

        return _mergeStrategy.Merge(databaseSettings, fileSettings, _serviceProvider);
    }

    /// <inheritdoc/>
    public async Task<SettingsConfigurationMetadata> GetMetadataAsync()
    {
        var databaseSettings = await GetDatabaseSettingsAsync();
        var fileSettings = GetFileConfigurationSettings();
        var effectiveSettings = _mergeStrategy.Merge(databaseSettings, fileSettings, _serviceProvider);

        var uiDisabled = effectiveSettings?.DisableUIConfiguration ?? false;

        var metadata = new SettingsConfigurationMetadata
        {
            SettingsType = typeof(TSettings),
            ConfigurationKey = _configurationKey,
            DisableUIConfiguration = uiDisabled,
        };

        foreach (var property in typeof(TSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead)
            {
                continue;
            }

            // Skip DisableUIConfiguration from property list
            if (property.Name == nameof(IConfigurableSettings.DisableUIConfiguration))
            {
                continue;
            }

            var dbValue = databaseSettings != null ? property.GetValue(databaseSettings) : null;
            var fileValue = fileSettings != null ? property.GetValue(fileSettings) : null;
            var effectiveValue = effectiveSettings != null ? property.GetValue(effectiveSettings) : null;

            var mergeInfo = AttributeBasedSettingsMergeStrategy<TSettings>.GetMergeInfo(property.Name);

            var source = _mergeStrategy.DeterminePropertySource(property.Name, dbValue, fileValue, uiDisabled);

            var propertyMetadata = new PropertyConfigurationMetadata
            {
                PropertyName = property.Name,
                DisplayName = mergeInfo?.Attribute?.DisplayName ?? property.Name,
                Source = source,
                DatabaseValue = dbValue,
                FileValue = fileValue,
                EffectiveValue = effectiveValue,
                Attribute = mergeInfo?.Attribute,
                DefaultValue = mergeInfo?.DefaultAttribute?.Value,
                IsSensitive = mergeInfo?.SensitiveAttribute != null,
                SensitiveAttribute = mergeInfo?.SensitiveAttribute,
                GroupName = mergeInfo?.GroupAttribute?.GroupName,
                GroupAttribute = mergeInfo?.GroupAttribute,
                PropertyType = property.PropertyType,
            };

            metadata.AddProperty(propertyMetadata);

            if (source == ConfigurationSource.ConfigurationFile)
            {
                metadata.IsConfiguredFromFile = true;
            }
        }

        return metadata;
    }

    /// <inheritdoc/>
    public Task<TSettings> GetDatabaseSettingsAsync()
        => _siteService.GetSettingsAsync<TSettings>();

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public ConfigurationPropertyAttribute GetPropertyAttribute(string propertyName)
    {
        var mergeInfo = AttributeBasedSettingsMergeStrategy<TSettings>.GetMergeInfo(propertyName);
        return mergeInfo?.Attribute;
    }

    /// <inheritdoc/>
    public bool CanConfigureViaFile(string propertyName)
    {
        var attribute = GetPropertyAttribute(propertyName);
        return attribute?.AllowFileConfiguration ?? true;
    }

    /// <inheritdoc/>
    public bool CanConfigureViaUI(string propertyName)
    {
        var fileSettings = GetFileConfigurationSettings();
        if (fileSettings?.DisableUIConfiguration == true)
        {
            return false;
        }

        var attribute = GetPropertyAttribute(propertyName);
        if (attribute == null)
        {
            return true;
        }

        if (!attribute.AllowUIConfiguration)
        {
            return false;
        }

        return attribute.MergeStrategy != PropertyMergeStrategy.FileOnly;
    }
}
