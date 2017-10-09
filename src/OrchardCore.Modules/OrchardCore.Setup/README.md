# Setup (OrchardCore.Setup)

## Recipe Parameters

During setup, all recipes have access to the setup screen values using these parameters:

| Parameter | Description |
| --- | --- |
| `SiteName` | The name of the site |
| `AdminUsername` | The username of the super user |
| `AdminEmail` | The email of the super user |
| `AdminPassword` | The password of the super user |
| `DatabaseProvider` | The database provider |
| `DatabaseConnectionString` | The connection string |
| `DatabaseTablePrefix` | The database table prefix |

These parameters can be used in the recipe using a scripted value like `[js: parameters('AdminUsername')]`.
