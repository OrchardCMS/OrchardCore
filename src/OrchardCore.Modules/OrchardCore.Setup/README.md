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

# Auto Setup (`OrchardCore.Setup.AutoSetup`)

## Recipe Parameters

When auto setup is enabled, it will look for the following configuration variables within the Modules / OrchardCore.Setup.AutoSetup configuration section; 

{
  Modules {
    "OrchardCore.Setup.AutoSetup" : {
        "PropA" : "ValueA"
    }
  }
}

| Parameter | Description |
| --- | --- |
| `SiteName` | The name of the site. |
| `AdminUsername` | The username of the super user. |
| `AdminEmail` | The email of the super user. |
| `AdminPassword` | The password of the super user. |
| `DatabaseProvider` | The database provider. |
| `DatabaseName` | The name of the database, required when CreateDatabase is true. |
| `DatabaseConnectionString` | The connection string. |
| `DatabaseTablePrefix` | The database table prefix. |
| 'RecipeName' | The name of the recipe to configure the site with. |
| 'CreateDatabase' | Indicate with true or false if the database should be created automatically. |

It will then perform the Setup of the site, without the need for user input.

Example configuration;
```json
{
  "Modules": {
    "OrchardCore.Setup.AutoSetup": {
      "SiteName": "AutoSetup Example",
      "SiteTimeZone": "Europe/Amsterdam",
      "AdminUsername": "Admin",
      "AdminEmail": "info@orchardproject.net",
      "AdminPassword": "xxxENTER_YOUR_PASSWORD_HERExxx",
      "DatabaseProvider": "Postgres",
      "DatabaseName": "OrchardCore",
      "DatabaseConnectionString": "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=orchardcore;EntityAdminDatabase=postgres;Pooling=true;",
      "DatabaseTablePrefix": "OrchardCore_",
      "RecipeName": "Agency",
      "CreateDatabase": "true"
    }
  }
}
```
