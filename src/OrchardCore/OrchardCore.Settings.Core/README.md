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

Add tag helpers to your `_ViewImports.cshtml`:

```razor
@addTagHelper *, OrchardCore.Settings.Core
@using OrchardCore.Settings
```

Use the tag helpers in your Razor view:

```razor
@model MySettingsViewModel

@* Configuration alert banner - shows when file configuration is active *@
<config-alert metadata="Model.Metadata" />

<fieldset @(Model.IsReadOnly ? "disabled" : "")>
    <div class="mb-3">
        <label asp-for="ApiEndpoint" class="form-label">
            API Endpoint
            @if (Model.IsPropertyOverridden("ApiEndpoint"))
            {
                <config-badge source="@ConfigurationSource.ConfigurationFile" class="ms-2" />
            }
        </label>
        @* Auto-disable input based on configuration metadata *@
        <input asp-for="ApiEndpoint" class="form-control"
               config-property="ApiEndpoint"
               config-metadata="Model.Metadata"
               config-readonly="Model.IsReadOnly" />
        @* Override warning for individual property *@
        <config-override-warning property="Model.GetPropertyMetadata(\"ApiEndpoint\")" />
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

## Tag Helpers

Add to your `_ViewImports.cshtml`:

```razor
@addTagHelper *, OrchardCore.Settings.Core
@using OrchardCore.Settings
```

### Available Tag Helpers

#### `<config-alert>`

Renders an alert banner when configuration file settings are active:

```razor
<config-alert metadata="Model.Metadata" />
<config-alert metadata="Model.Metadata" show-overridden-properties="false" />
```

| Attribute | Description |
|-----------|-------------|
| `metadata` | The `SettingsConfigurationMetadata` object |
| `show-overridden-properties` | Whether to list overridden properties (default: true) |

#### `<config-badge>`

Renders a badge indicating the configuration source:

```razor
<config-badge source="@ConfigurationSource.ConfigurationFile" />
<config-badge source="@Model.GetPropertySource(\"PropertyName\")" />
```

| Attribute | Description |
|-----------|-------------|
| `source` | The `ConfigurationSource` enum value |
| `size` | Badge size (default: "sm") |

#### `<config-override-warning>`

Renders a warning when a property is overridden by configuration file:

```razor
<config-override-warning property="Model.GetPropertyMetadata(\"PropertyName\")" />
```

| Attribute | Description |
|-----------|-------------|
| `property` | The `PropertyConfigurationMetadata` object |

Note: This tag helper automatically suppresses output for properties using `Merge` strategy, since those values are combined rather than overridden.

#### `<config-property-info>`

Renders comprehensive property info including badge and effective value:

```razor
<config-property-info property-name="PropertyName" metadata="@Model.Metadata" />
```

| Attribute | Description |
|-----------|-------------|
| `property-name` | The property name |
| `metadata` | The `SettingsConfigurationMetadata` object |
| `show-effective-value` | Whether to show effective value (default: true) |

#### `config-property` Attribute

Auto-disables inputs based on configuration metadata:

```razor
<input asp-for="PropertyName"
       config-property="PropertyName"
       config-metadata="Model.Metadata"
       config-readonly="Model.IsReadOnly" />
```

Works with `<input>`, `<select>`, and `<textarea>` elements.

## Built-in Merge Functions

For custom scenarios, use built-in merge functions or create your own:

- `ArrayMergeFunction` - Combines unique array values
- `DictionaryMergeFunction` - Merges dictionaries (file keys override)
- `MaxValueMergeFunction` - Takes maximum of numeric values
- `MinValueMergeFunction` - Takes minimum of numeric values
- `BooleanOrMergeFunction` - OR logic for booleans
- `BooleanAndMergeFunction` - AND logic for booleans
- `StringConcatMergeFunction` - Concatenates strings

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
IEnumerable<PropertyConfigurationMetadata> GetUIConfigurableProperties();
IEnumerable<PropertyConfigurationMetadata> GetSensitiveProperties();
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
PropertyMergeStrategy MergeStrategy { get; }
string GetMaskedValue();
string GetDisplayValue();
```

### ConfigurableSettingsViewModel<TSettings>

Base class for view models with helper methods:

```csharp
SettingsConfigurationMetadata Metadata { get; set; }
bool HasFileOverrides { get; }
bool IsReadOnly { get; }
bool IsPropertyOverridden(string propertyName);
ConfigurationSource GetPropertySource(string propertyName);
PropertyConfigurationMetadata GetPropertyMetadata(string propertyName);
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

See `OrchardCore.ReverseProxy` module for a complete reference implementation demonstrating:

- Settings class with `IConfigurableSettings` and `[ConfigurationProperty]` attributes
- View model extending `ConfigurableSettingsViewModel<T>`
- Display driver extending `ConfigurableSiteSettingsDisplayDriver<T, TViewModel>`
- View using tag helpers (`<config-alert>`, `<config-badge>`, `<config-override-warning>`, `config-property`)
- Service registration in Startup
