# Configurable Settings Infrastructure

A **generic, reusable infrastructure** for OrchardCore modules that enables settings to be configured through both Admin UI and `appsettings.json`, with **declarative, attribute-based control** over how these sources merge.

## Overview

This infrastructure solves the problem where configuration file settings don't properly override database/UI settings, and provides transparency about which configuration source is active.

### Key Features

- **Generic Infrastructure** - Write once, reuse everywhere
- **Declarative Control** - Attributes define behavior, not code
- **Transparency** - UI shows exactly what's active and why
- **Flexibility** - Support multiple merge strategies for different scenarios
- **Security-First** - Built-in support for secrets and sensitive data
- **Backward Compatible** - Existing modules continue to work

## Quick Start

### 1. Update Your Settings Class

Add `IConfigurableSettings` interface and `ConfigurationProperty` attributes:

```csharp
public class MySettings : IConfigurableSettings
{
    [ConfigurationProperty(
        MergeStrategy = PropertyMergeStrategy.FileOverridesDatabase,
        DisplayName = "API Endpoint",
        Description = "The URL of the API endpoint.")]
    public string ApiEndpoint { get; set; }

    [ConfigurationProperty(
        MergeStrategy = PropertyMergeStrategy.Merge,
        DisplayName = "Allowed Origins")]
    public string[] AllowedOrigins { get; set; } = [];

    [ConfigurationProperty(
        MergeStrategy = PropertyMergeStrategy.FileOnly,
        AllowUIConfiguration = false)]
    [SensitiveConfiguration]
    public string ApiKey { get; set; }

    // Required by IConfigurableSettings
    public bool DisableUIConfiguration { get; set; }
}
```

### 2. Update Your View Model

Extend `ConfigurableSettingsViewModel<T>`:

```csharp
public class MySettingsViewModel : ConfigurableSettingsViewModel<MySettings>
{
    public string ApiEndpoint { get; set; }
    public string AllowedOriginsText { get; set; }
}
```

### 3. Update Your Display Driver

Extend `ConfigurableSiteSettingsDisplayDriver<TSettings, TViewModel>`:

```csharp
public sealed class MySettingsDisplayDriver 
    : ConfigurableSiteSettingsDisplayDriver<MySettings, MySettingsViewModel>
{
    public MySettingsDisplayDriver(
        IConfigurableSettingsService<MySettings> settingsService,
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
        : base(settingsService, shellReleaseManager, httpContextAccessor, authorizationService)
    {
    }

    protected override string SettingsGroupId => "MyModule";
    protected override string EditShapeType => "MySettings_Edit";
    protected override Permission RequiredPermission => MyPermissions.ManageSettings;

    protected override void PopulateViewModel(
        MySettingsViewModel model, 
        MySettings databaseSettings, 
        MySettings effectiveSettings)
    {
        model.ApiEndpoint = databaseSettings.ApiEndpoint;
        model.AllowedOriginsText = string.Join("\n", databaseSettings.AllowedOrigins ?? []);
    }

    protected override void UpdateSettings(
        MySettings settings, 
        MySettingsViewModel model, 
        SettingsConfigurationMetadata metadata)
    {
        if (ShouldUpdateProperty(nameof(MySettings.ApiEndpoint), metadata))
        {
            settings.ApiEndpoint = model.ApiEndpoint;
        }

        if (ShouldUpdateProperty(nameof(MySettings.AllowedOrigins), metadata))
        {
            settings.AllowedOrigins = model.AllowedOriginsText?
                .Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? [];
        }
    }
}
```

### 4. Register Services in Startup

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // Register the configurable settings service
    services.AddConfigurableSettings<MySettings>("OrchardCore_MyModule");
    
    // Register your display driver
    services.AddSiteDisplayDriver<MySettingsDisplayDriver>();
}
```

### 5. Update Your View

Use the metadata in your Razor view:

```razor
@model MySettingsViewModel

@if (Model.HasFileOverrides)
{
    <div class="alert alert-info">
        Some settings are configured via appsettings.json.
    </div>
}

<fieldset @(Model.IsReadOnly ? "disabled" : "")>
    <div class="mb-3">
        <label asp-for="ApiEndpoint" class="form-label">
            API Endpoint
            @if (Model.IsPropertyOverridden("ApiEndpoint"))
            {
                <span class="badge text-bg-info">Config File</span>
            }
        </label>
        <input asp-for="ApiEndpoint" class="form-control" />
    </div>
</fieldset>
```

### 6. Configure via appsettings.json

```json
{
  "OrchardCore_MyModule": {
    "ApiEndpoint": "https://api.example.com",
    "AllowedOrigins": ["https://example.com", "https://www.example.com"],
    "ApiKey": "secret-key-from-vault",
    "DisableUIConfiguration": false
  }
}
```

## Merge Strategies

| Strategy | Behavior | Use Case |
|----------|----------|----------|
| **FileOverridesDatabase** | File wins when set (default) | Production environment overrides |
| **DatabaseOverridesFile** | UI wins when set | Developer overrides defaults |
| **FileAsDefault** | File only used when DB empty | Providing fallback values |
| **DatabaseAsDefault** | DB only used when file empty | Rare scenarios |
| **Merge** | Combine both sources | Arrays, collections, lists |
| **DatabaseOnly** | File configuration disabled | UI-managed settings |
| **FileOnly** | UI configuration disabled | Secrets, production-only |
| **Custom** | User-defined logic | Complex business rules |

## Attributes

### `[ConfigurationProperty]`

Controls merge behavior for a property:

- `MergeStrategy` - How to combine file and database values
- `AllowFileConfiguration` - Can be set via appsettings.json
- `AllowUIConfiguration` - Can be set via Admin UI
- `DisplayName` - Friendly name for UI
- `Description` - Help text for UI
- `CustomMergeFunction` - Type of custom merge logic
- `Priority` - Conflict resolution order

### `[SensitiveConfiguration]`

Marks properties containing sensitive data:

- `MaskCharacter` - Character for masking display (default: '•')
- `VisibleCharacters` - How many chars to show unmasked (default: 4)
- `AllowReveal` - Whether full value can be shown in UI

### `[DefaultConfigurationValue]`

Provides fallback when both sources are empty.

### `[ConfigurationGroup]`

Groups related properties in UI.

## Security Best Practices

### Secrets Management

For sensitive properties like API keys:

```csharp
[ConfigurationProperty(
    MergeStrategy = PropertyMergeStrategy.FileOnly,
    AllowUIConfiguration = false)]
[SensitiveConfiguration]
public string ApiKey { get; set; }
```

### Production Lockdown

Disable all UI configuration in production:

```json
{
  "OrchardCore_MyModule": {
    "DisableUIConfiguration": true
  }
}
```

## Built-in Merge Functions

For custom scenarios, use built-in merge functions or create your own:

- `ArrayMergeFunction` - Combines unique array values
- `DictionaryMergeFunction` - Merges dictionaries (file keys override)
- `MaxValueMergeFunction` - Takes maximum of numeric values
- `MinValueMergeFunction` - Takes minimum of numeric values
- `BooleanOrMergeFunction` - OR logic for booleans
- `BooleanAndMergeFunction` - AND logic for booleans
- `StringConcatMergeFunction` - Concatenates strings

## Tag Helpers

Add to your `_ViewImports.cshtml`:

```razor
@addTagHelper *, OrchardCore.Settings.Core
```

### Available Tag Helpers

- `<config-badge source="@Model.GetPropertySource("PropertyName")" />` - Renders source badge
- `<config-property-info property-name="PropertyName" metadata="@Model.Metadata" />` - Full property info
- `config-property` attribute on inputs - Auto-disables based on configuration

## Partial Views

Use shared partial views:

- `_ConfigurationAlert.cshtml` - Shows banner when file configuration is active
- `_PropertyOverrideWarning.cshtml` - Shows warning for individual overridden property

## API Reference

### IConfigurableSettingsService<TSettings>

```csharp
Task<TSettings> GetEffectiveSettingsAsync();
Task<SettingsConfigurationMetadata> GetMetadataAsync();
Task<TSettings> GetDatabaseSettingsAsync();
TSettings GetFileConfigurationSettings();
ConfigurationPropertyAttribute GetPropertyAttribute(string propertyName);
bool CanConfigureViaFile(string propertyName);
bool CanConfigureViaUI(string propertyName);
```

### SettingsConfigurationMetadata

```csharp
bool IsConfiguredFromFile { get; }
bool DisableUIConfiguration { get; }
IReadOnlyDictionary<string, PropertyConfigurationMetadata> Properties { get; }
bool IsPropertyOverridden(string propertyName);
ConfigurationSource GetPropertySource(string propertyName);
IEnumerable<PropertyConfigurationMetadata> GetOverriddenProperties();
```

### PropertyConfigurationMetadata

```csharp
string PropertyName { get; }
ConfigurationSource Source { get; }
object DatabaseValue { get; }
object FileValue { get; }
object EffectiveValue { get; }
bool IsOverriddenByFile { get; }
bool CanConfigureViaUI { get; }
bool IsSensitive { get; }
string GetMaskedValue();
```

## Migration from Old Pattern

### Before

```csharp
// Manual configuration service
public class MyService
{
    public Task<MySettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<MySettings>();
}
```

### After

```csharp
// Use configurable settings service
public class MyService
{
    private readonly IConfigurableSettingsService<MySettings> _settingsService;
    
    public Task<MySettings> GetSettingsAsync()
        => _settingsService.GetEffectiveSettingsAsync();
}
```

## Reference Implementation

See `OrchardCore.ReverseProxy` module for a complete reference implementation.
