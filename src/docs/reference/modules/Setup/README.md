# Setup (`OrchardCore.Setup`)

## Recipe Parameters

During setup, all recipes have access to the setup screen values using these parameters:

| Parameter | Description |
| --- | --- |
| `SiteName` | The name of the site. |
| `AdminUsername` | The username of the super user. |
| `AdminEmail` | The email of the super user. |
| `AdminPassword` | The password of the super user. |
| `DatabaseProvider` | The database provider. |
| `DatabaseConnectionString` | The connection string. |
| `DatabaseTablePrefix` | The database table prefix. |

These parameters can be used in the recipe using a scripted value like `[js: parameters('AdminUsername')]`.

### Custom Parameters

Custom parameters can also be used in the recipe, using a scripted value like `[js: configuration('CustomParameterKey')]`.

The value will be retrieved from the `appsettings.json` tenant file.

```json
    {
        "ConnectionString": "...",
        "DatabaseProvider": "Sqlite",
        "TablePrefix": "Test",
        "CustomParameterKey": "Custom Parameter Value"
    }
```

## Configuration

The following configuration values are used by default and can be customized:

```json
    "OrchardCore_Setup": {
      "DefaultCulture": "", // When using "" the system OS culture will be used
      "SupportedCultures": [
        "ar", 
        "cs", 
        "da", 
        "de", 
        "el", 
        "en", 
        "es", 
        "fa", 
        "fi", 
        "fr", 
        "he", 
        "hr", 
        "hu", 
        "id", 
        "it", 
        "ja", 
        "ko", 
        "lt", 
        "mk", 
        "nl", 
        "pl", 
        "pt", 
        "ru", 
        "sk", 
        "sl", 
        "sr-cyrl-rs", 
        "sr-latn-rs", 
        "sv", 
        "tr", 
        "uk", 
        "vi", 
        "zh-CN", 
        "zh-TW"
      ] // "" value (InvariantCulture) is not supported for these
    }
```

| Key | Description |
| --- | --- |
| `DefaultCulture` | The default culture that will be used for the setup screen. |
| `SupportedCultures` | The list of the supported cultures for the setup screen. |