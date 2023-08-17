# Setup (`OrchardCore.Setup`)

When you begin with an empty site, a start screen allows you to setup the different parameters as the Title, the selected database, or the recipe that will be used to generate the site. This is done by the Setup module.

## Recipe Parameters

During setup, all recipes have access to the setup screen values using these parameters:

| Parameter | Description |
| --- | --- |
| `SiteName` | The name of the site. |
| `AdminUserId` | The user id of the super user. |
| `AdminUsername` | The username of the super user. |
| `AdminEmail` | The email of the super user. |
| `AdminPassword` | The password of the super user. |
| `DatabaseProvider` | The database provider. |
| `DatabaseConnectionString` | The connection string. |
| `DatabaseTablePrefix` | The database table prefix. |

These parameters can be used in the recipe using a scripted value like `[js: parameters('AdminUserId')]`.

### Recipe Configuration Keys

Custom configuration keys can also be used in the recipe, using a scripted key value like `[js: configuration('CustomConfigurationKey')]`.

The key will be retrieved from the current [IShellConfiguration](../../core/Configuration/README.md). 

For example to provide a key for a tenant

```json
    {
        "ConnectionString": "...",
        "DatabaseProvider": "Sqlite",
        "TablePrefix": "Test",
        "CustomConfigurationKey": "Custom Configuration Value"
    }
```

Other configuration keys can also be used, i.e. from the hosts `appsettings.json` 

`[js: configuration('OrchardCore_Admin:AdminUrlPrefix', 'Admin')]`

In this example we also provide a default value, which will be used if the key is not found.

```json
    {
        "OrchardCore_Admin" : {
            "AdminUrlPrefix" : "MyAdmin"
        }
    }
```

## Setup Configuration

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

## CDN disabled by default

The `UseCdn` option, configured in the _Configuration -> Settings -> General_ section, is disabled by default.
This is to allow access to resources when an internet connection is not available or in countries like China, where CDNs are not always accessible.  

!!! note
    It is recommended to enable the CDN setting after setup.
    
## Additional information
Please refer to separate sections for additional information on setup:

- [Auto Setup - how to predefine setup parameters when deploying an empty site](../AutoSetup/README.md)

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/usjGbjwbmNo" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
