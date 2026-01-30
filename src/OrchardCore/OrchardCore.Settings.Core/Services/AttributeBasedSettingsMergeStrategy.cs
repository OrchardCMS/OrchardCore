using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Settings.Services;

/// <summary>
/// Default implementation of <see cref="ISettingsMergeStrategy{TSettings}"/> that uses
/// <see cref="ConfigurationPropertyAttribute"/> to determine merge behavior.
/// </summary>
/// <typeparam name="TSettings">The type of settings to merge.</typeparam>
public class AttributeBasedSettingsMergeStrategy<TSettings> : ISettingsMergeStrategy<TSettings>
    where TSettings : class, IConfigurableSettings, new()
{
    private static readonly ConcurrentDictionary<string, PropertyMergeInfo> _propertyCache = new(StringComparer.OrdinalIgnoreCase);
    private static bool _cacheInitialized;
    private static readonly object _cacheLock = new();

    public TSettings Merge(TSettings databaseSettings, TSettings fileSettings, IServiceProvider serviceProvider)
    {
        EnsureCacheInitialized();

        databaseSettings ??= new TSettings();
        fileSettings ??= new TSettings();

        // Check if UI configuration is disabled globally
        var uiDisabled = fileSettings.DisableUIConfiguration;

        var merged = new TSettings
        {
            DisableUIConfiguration = uiDisabled,
        };

        foreach (var propertyInfo in typeof(TSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                continue;
            }

            // Skip DisableUIConfiguration as it's handled separately
            if (propertyInfo.Name == nameof(IConfigurableSettings.DisableUIConfiguration))
            {
                continue;
            }

            var dbValue = propertyInfo.GetValue(databaseSettings);
            var fileValue = propertyInfo.GetValue(fileSettings);

            var mergeInfo = _propertyCache.GetValueOrDefault(propertyInfo.Name);
            var mergedValue = MergePropertyValue(propertyInfo, dbValue, fileValue, mergeInfo, uiDisabled, serviceProvider);

            propertyInfo.SetValue(merged, mergedValue);
        }

        return merged;
    }

    public ConfigurationSource DeterminePropertySource(string propertyName, object databaseValue, object fileValue, bool uiDisabled)
    {
        EnsureCacheInitialized();

        var mergeInfo = _propertyCache.GetValueOrDefault(propertyName);
        var attribute = mergeInfo?.Attribute;
        var strategy = attribute?.MergeStrategy ?? PropertyMergeStrategy.FileOverridesDatabase;

        // Check if file configuration is allowed
        var allowFile = attribute?.AllowFileConfiguration ?? true;
        var allowUI = attribute?.AllowUIConfiguration ?? true;

        var hasDbValue = !IsNullOrEmpty(databaseValue);
        var hasFileValue = !IsNullOrEmpty(fileValue);

        // If UI is globally disabled, always use file if available
        if (uiDisabled)
        {
            return hasFileValue ? ConfigurationSource.ConfigurationFile : ConfigurationSource.Default;
        }

        // Handle FileOnly strategy
        if (strategy == PropertyMergeStrategy.FileOnly || !allowUI)
        {
            return hasFileValue ? ConfigurationSource.ConfigurationFile : ConfigurationSource.Default;
        }

        // Handle DatabaseOnly strategy
        if (strategy == PropertyMergeStrategy.DatabaseOnly || !allowFile)
        {
            return hasDbValue ? ConfigurationSource.Database : ConfigurationSource.Default;
        }

        // Handle standard strategies
        return strategy switch
        {
            PropertyMergeStrategy.FileOverridesDatabase => hasFileValue
                ? ConfigurationSource.ConfigurationFile
                : hasDbValue ? ConfigurationSource.Database : ConfigurationSource.Default,

            PropertyMergeStrategy.DatabaseOverridesFile => hasDbValue
                ? ConfigurationSource.Database
                : hasFileValue ? ConfigurationSource.ConfigurationFile : ConfigurationSource.Default,

            PropertyMergeStrategy.FileAsDefault => hasDbValue
                ? ConfigurationSource.Database
                : hasFileValue ? ConfigurationSource.ConfigurationFile : ConfigurationSource.Default,

            PropertyMergeStrategy.DatabaseAsDefault => hasFileValue
                ? ConfigurationSource.ConfigurationFile
                : hasDbValue ? ConfigurationSource.Database : ConfigurationSource.Default,

            PropertyMergeStrategy.Merge or PropertyMergeStrategy.Custom => hasFileValue
                ? ConfigurationSource.ConfigurationFile
                : hasDbValue ? ConfigurationSource.Database : ConfigurationSource.Default,

            _ => hasFileValue
                ? ConfigurationSource.ConfigurationFile
                : hasDbValue ? ConfigurationSource.Database : ConfigurationSource.Default,
        };
    }

    private object MergePropertyValue(
        PropertyInfo propertyInfo,
        object databaseValue,
        object fileValue,
        PropertyMergeInfo mergeInfo,
        bool uiDisabled,
        IServiceProvider serviceProvider)
    {
        var attribute = mergeInfo?.Attribute;
        var strategy = attribute?.MergeStrategy ?? PropertyMergeStrategy.FileOverridesDatabase;

        var allowFile = attribute?.AllowFileConfiguration ?? true;
        var allowUI = attribute?.AllowUIConfiguration ?? true;

        var hasDbValue = !IsNullOrEmpty(databaseValue);
        var hasFileValue = !IsNullOrEmpty(fileValue);

        // If UI is globally disabled, always use file value
        if (uiDisabled && allowFile)
        {
            return hasFileValue ? fileValue : GetDefaultValue(mergeInfo, propertyInfo.PropertyType);
        }

        // Handle FileOnly strategy
        if (strategy == PropertyMergeStrategy.FileOnly || !allowUI)
        {
            return hasFileValue ? fileValue : GetDefaultValue(mergeInfo, propertyInfo.PropertyType);
        }

        // Handle DatabaseOnly strategy
        if (strategy == PropertyMergeStrategy.DatabaseOnly || !allowFile)
        {
            return hasDbValue ? databaseValue : GetDefaultValue(mergeInfo, propertyInfo.PropertyType);
        }

        // Handle Custom strategy
        if (strategy == PropertyMergeStrategy.Custom && attribute?.CustomMergeFunction != null)
        {
            return ExecuteCustomMerge(attribute.CustomMergeFunction, databaseValue, fileValue, propertyInfo, attribute, uiDisabled, serviceProvider);
        }

        // Handle Merge strategy
        if (strategy == PropertyMergeStrategy.Merge)
        {
            return MergeValues(databaseValue, fileValue, propertyInfo.PropertyType);
        }

        // Handle standard strategies
        return strategy switch
        {
            PropertyMergeStrategy.FileOverridesDatabase => hasFileValue ? fileValue
                : hasDbValue ? databaseValue
                : GetDefaultValue(mergeInfo, propertyInfo.PropertyType),

            PropertyMergeStrategy.DatabaseOverridesFile => hasDbValue ? databaseValue
                : hasFileValue ? fileValue
                : GetDefaultValue(mergeInfo, propertyInfo.PropertyType),

            PropertyMergeStrategy.FileAsDefault => hasDbValue ? databaseValue
                : hasFileValue ? fileValue
                : GetDefaultValue(mergeInfo, propertyInfo.PropertyType),

            PropertyMergeStrategy.DatabaseAsDefault => hasFileValue ? fileValue
                : hasDbValue ? databaseValue
                : GetDefaultValue(mergeInfo, propertyInfo.PropertyType),

            _ => hasFileValue ? fileValue
                : hasDbValue ? databaseValue
                : GetDefaultValue(mergeInfo, propertyInfo.PropertyType),
        };
    }

    private object ExecuteCustomMerge(
        Type mergeFunctionType,
        object databaseValue,
        object fileValue,
        PropertyInfo propertyInfo,
        ConfigurationPropertyAttribute attribute,
        bool uiDisabled,
        IServiceProvider serviceProvider)
    {
        var mergeFunction = serviceProvider.GetService(mergeFunctionType) as IPropertyMergeFunction;
        if (mergeFunction == null)
        {
            // Try to create instance if not registered
            try
            {
                mergeFunction = Activator.CreateInstance(mergeFunctionType) as IPropertyMergeFunction;
            }
            catch
            {
                // Fall back to file-overrides-database if custom merge fails
                return !IsNullOrEmpty(fileValue) ? fileValue : databaseValue;
            }
        }

        if (mergeFunction == null)
        {
            return !IsNullOrEmpty(fileValue) ? fileValue : databaseValue;
        }

        var context = new PropertyMergeContext
        {
            PropertyName = propertyInfo.Name,
            PropertyType = propertyInfo.PropertyType,
            SettingsType = typeof(TSettings),
            Attribute = attribute,
            DisableUIConfiguration = uiDisabled,
            ServiceProvider = serviceProvider,
        };

        return mergeFunction.Merge(databaseValue, fileValue, context);
    }

    private static object MergeValues(object databaseValue, object fileValue, Type propertyType)
    {
        // If only one has value, use it
        var hasDbValue = !IsNullOrEmpty(databaseValue);
        var hasFileValue = !IsNullOrEmpty(fileValue);

        if (!hasDbValue && !hasFileValue)
        {
            return GetTypeDefault(propertyType);
        }

        if (!hasDbValue)
        {
            return fileValue;
        }

        if (!hasFileValue)
        {
            return databaseValue;
        }

        // Both have values - merge based on type
        if (propertyType.IsArray)
        {
            return MergeArrays(databaseValue, fileValue, propertyType);
        }

        if (propertyType.IsGenericType)
        {
            var genericDef = propertyType.GetGenericTypeDefinition();

            if (genericDef == typeof(List<>) || genericDef == typeof(IList<>) ||
                genericDef == typeof(ICollection<>) || genericDef == typeof(IEnumerable<>))
            {
                return MergeLists(databaseValue, fileValue, propertyType);
            }

            if (genericDef == typeof(Dictionary<,>) || genericDef == typeof(IDictionary<,>))
            {
                return MergeDictionaries(databaseValue, fileValue, propertyType);
            }
        }

        // For non-collection types, file takes precedence in merge mode
        return fileValue;
    }

    private static object MergeArrays(object databaseValue, object fileValue, Type propertyType)
    {
        var elementType = propertyType.GetElementType();
        var dbArray = databaseValue as Array;
        var fileArray = fileValue as Array;

        var combined = new HashSet<object>();

        // Add file values first (they take precedence)
        if (fileArray != null)
        {
            foreach (var item in fileArray)
            {
                combined.Add(item);
            }
        }

        // Add unique database values
        if (dbArray != null)
        {
            foreach (var item in dbArray)
            {
                combined.Add(item);
            }
        }

        var result = Array.CreateInstance(elementType, combined.Count);
        var index = 0;
        foreach (var item in combined)
        {
            result.SetValue(item, index++);
        }

        return result;
    }

    private static object MergeLists(object databaseValue, object fileValue, Type propertyType)
    {
        var elementType = propertyType.GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(elementType);
        var result = (System.Collections.IList)Activator.CreateInstance(listType);

        var combined = new HashSet<object>();

        // Add file values first
        if (fileValue is System.Collections.IEnumerable fileEnumerable)
        {
            foreach (var item in fileEnumerable)
            {
                if (combined.Add(item))
                {
                    result.Add(item);
                }
            }
        }

        // Add unique database values
        if (databaseValue is System.Collections.IEnumerable dbEnumerable)
        {
            foreach (var item in dbEnumerable)
            {
                if (combined.Add(item))
                {
                    result.Add(item);
                }
            }
        }

        return result;
    }

    private static object MergeDictionaries(object databaseValue, object fileValue, Type propertyType)
    {
        var keyType = propertyType.GetGenericArguments()[0];
        var valueType = propertyType.GetGenericArguments()[1];
        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var result = (System.Collections.IDictionary)Activator.CreateInstance(dictType);

        // Add database values first
        if (databaseValue is System.Collections.IDictionary dbDict)
        {
            foreach (System.Collections.DictionaryEntry entry in dbDict)
            {
                result[entry.Key] = entry.Value;
            }
        }

        // File values override database values (added second)
        if (fileValue is System.Collections.IDictionary fileDict)
        {
            foreach (System.Collections.DictionaryEntry entry in fileDict)
            {
                result[entry.Key] = entry.Value;
            }
        }

        return result;
    }

    private static object GetDefaultValue(PropertyMergeInfo mergeInfo, Type propertyType)
    {
        if (mergeInfo?.DefaultAttribute?.Value != null)
        {
            var defaultValue = mergeInfo.DefaultAttribute.Value;

            // Try to convert if types don't match
            if (defaultValue.GetType() != propertyType)
            {
                try
                {
                    return Convert.ChangeType(defaultValue, propertyType);
                }
                catch
                {
                    return GetTypeDefault(propertyType);
                }
            }

            return defaultValue;
        }

        return GetTypeDefault(propertyType);
    }

    private static object GetTypeDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    private static bool IsNullOrEmpty(object value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is string str)
        {
            return string.IsNullOrEmpty(str);
        }

        if (value is Array array)
        {
            return array.Length == 0;
        }

        if (value is System.Collections.ICollection collection)
        {
            return collection.Count == 0;
        }

        return false;
    }

    private static void EnsureCacheInitialized()
    {
        if (_cacheInitialized)
        {
            return;
        }

        lock (_cacheLock)
        {
            if (_cacheInitialized)
            {
                return;
            }

            foreach (var property in typeof(TSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var mergeInfo = new PropertyMergeInfo
                {
                    PropertyInfo = property,
                    Attribute = property.GetCustomAttribute<ConfigurationPropertyAttribute>(),
                    DefaultAttribute = property.GetCustomAttribute<DefaultConfigurationValueAttribute>(),
                    SensitiveAttribute = property.GetCustomAttribute<SensitiveConfigurationAttribute>(),
                    GroupAttribute = property.GetCustomAttribute<ConfigurationGroupAttribute>(),
                };

                _propertyCache[property.Name] = mergeInfo;
            }

            _cacheInitialized = true;
        }
    }

    internal static PropertyMergeInfo GetMergeInfo(string propertyName)
    {
        EnsureCacheInitialized();
        return _propertyCache.GetValueOrDefault(propertyName);
    }

    internal class PropertyMergeInfo
    {
        public PropertyInfo PropertyInfo { get; set; }
        public ConfigurationPropertyAttribute Attribute { get; set; }
        public DefaultConfigurationValueAttribute DefaultAttribute { get; set; }
        public SensitiveConfigurationAttribute SensitiveAttribute { get; set; }
        public ConfigurationGroupAttribute GroupAttribute { get; set; }
    }
}
