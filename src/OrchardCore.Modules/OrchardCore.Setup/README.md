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

## Configuration
`OrchardCore.Setup` can be configured through `appsettings.json` as follows:

```json
    "OrchardCore.Setup": {
        "DefaultCulture": "",
        "SupportedCultures": [ "en" ]
    }
```

| Key | Description |
| --- | --- |
| `DefaultCulture` | The default culture that will be used for the setup screen. |
| `SupportedCultures` | The list of the supported cultures for the setup screen. |