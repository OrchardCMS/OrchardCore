# Configuration

Orchard Core extends ASP.NET Core `IConfiguration` with `IShellConfiguration` to allow tenant-specific configuration on top of the application-wide one.

To learn more about ASP.NET Core `IConfiguration` visit <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration>.

Note that while this documentation page explains configuration happening in the root web app project on the example of `OrchardCore.Cms.Web.csproj` if you use Orchard from NuGet packages in your own web app then same is available in that web app project too.


## Config Sources

Orchard Core supports a hierarchy of Configuration Sources

* The `Startup` ASP.NET Core Project, e.g. `OrchardCore.Cms.Web.csproj`, `appsettings.json`, or by environment  `appsettings.Development.json`.
* Global Tenant Configuration `App_Data/appsettings.json`, or by environment `App_Data/appsettings.Development.json`.
* Individual Tenant Configuration files located under each Tenant Folder in the `App_Data/Sites/{tenant_name}/appsettings.json` folder. **Note:** These are mutable files, and do not support an Environment version.
* Environment Variables, or AppSettings as Environment Variables via Azure.

The Configuration Sources are loaded in the above order, and settings lower in the hierarchy will override values configured higher up, i.e. an Global Tenant value will always be overridden by an Environment Variable.

!!! note 
    The `IShellConfiguration` patterns in the `appsettings.json` examples below will only work for modules that specifically support such configuration. You can check out the given module's code or documentation to see if this is the case.

### `IShellConfiguration` in the `OrchardCore.Cms.Web.csproj` Startup Project

Orchard Core stores all Configuration data under the `OrchardCore` section in `appsettings.json` files:

```json
{
  "OrchardCore": {
      ... module configurations ...
  }
}
```

Each Orchard Core module has its own configuration section under the `OrchardCore` section:

```json
{
  "OrchardCore": {
    "OrchardCore_Media": {
      ... individual module configuration ...
    }
  }
}
```

See the `appsettings.json` file for more examples.

### Tenant Preconfiguration

To pre configure the setup values for a tenant before it has been created you can specify a section named for the tenant,
with a `State` value of `Uninitialized`

```json
{
  "OrchardCore": {
    "MyTenant": {
      "State": "Uninitialized",
      "RequestUrlPrefix": "mytenant",
      "ConnectionString": "...",
      "DatabaseProvider": "SqlConnection"
    }
  }
}
```

The preconfigured tenant will then appear in the `Tenants` list in the admin, and these values will be used when the tenant is setup.

### Tenant Postconfiguration

To configure the values for a tenant after it has been created you can specify a section named for the tenant,
without having to provide a state value.

```json
{
  "OrchardCore": {
    "Default": {
      "OrchardCore_Media": {
        ... specific tenant configuration ...
      }
    }
  }
}
```

### Global tenant data access configuration

What if you want all tenants to access the same database? The corresponding configuration can be kept in a single place, as opposed to setting up the same connection string for all tenants one by one, as following:

```json
{
  "OrchardCore": {
    "ConnectionString": "...",
    "DatabaseProvider": "SqlConnection",
    "Default" : {
      "State": "Uninitialized",
      "TablePrefix": "Default"
    }
  }
}
```

Notes on the above configuration:

- Be aware that while you can use the same configuration keys for tenants, as demonstrated previously, this is in the root of the `OrchardCore` section.
- Add the connection string for the database to be used by all tenants.
- `DatabaseProvider` should correspond to the database engine used, the sample being one for SQL Server.
- `TablePrefix` needs to be configured to the prefix used by the Default tenant so tables can be separated for each tenant (otherwise just the Default tenant's tables would lack prefixes). Other tenants should then be set up with a different prefix.

This way, the app can be easily moved between environments (like a staging and production one) by configuring the corresponding database's settings in the given environment. Tenants' shell settings won't contain this information, all tenants will use the same, global configuration.

A related topic is [Shells Configuration Providers](../Shells/README.md). See especially the section about Database Shells Configuration Provider, on how to keep all shell configuration in the database.

### `IOptions` Configuration

You can also configure `IOptions` from code in the web project's `Startup` class as explained in the [ASP.NET documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options). 

A lot of Orchard Core features are configured through the admin UI with site settings stored in the database and/or expose configuration via `IOptions`. If you wish to override the site settings or default settings, you can do this with your own configuration code.

For example, the Email module allows SMTP configuration via the `SmtpSettings` class which by default is populated from the given tenant's site settings, as set on the admin. 
However, you can override the site settings from the `Startup` class like this (note that we use `PostConfigure` to override the site settings values but if the module doesn't use site settings you can just use `Configure`):

```csharp
services
    .AddOrchardCms()
    .ConfigureServices(tenantServices =>
        tenantServices.PostConfigure<SmtpSettings>(settings =>
        {
            settings.Port = 255;
        }));

// Or if you want to make use of IShellConfiguration as seen above:
services
    .AddOrchardCms()
    .ConfigureServices((tenantServices, serviceProvider) =>
    {
        // Instead of IShellConfiguration you could fetch the configuration 
        // values from an injected IConfiguration instance here too. While that 
        // would also allow you to access standard ASP.NET Core configuration 
        // keys it won't have support for all the hierarchical sources 
        // detailed above.
        var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>();
        var password = shellConfiguration.GetValue<string>("SmtpSettings:Password");

        tenantServices.PostConfigure<SmtpSettings>(settings =>
        {
            settings.Password = password;
        });
    });
```

!!! note 
    Such configuration for `SmtpSettings` is already available via the `ConfigureEmailSettings` extension method, see [Email Configuration](../../modules/Email/README.md).

This will make the SMTP port use this configuration despite any other value defined in site settings. The second example's configuration value can come from e.g. an `appsettings.json` file like below:

```json
{
  "OrchardCore": {
    "SmtpSettings": {
      "Password":  "password"
    }
  }
}
```

!!! note 
    On the admin there will be no indication that this override happened, and the value displayed there will still be 
    the one configured in site settings, so if you choose to do this you'll need to let your users know.

### `ORCHARD_APP_DATA` Environment Variable

The location of the `App_Data` folder can be configured by setting the `ORCHARD_APP_DATA` environment variable. 
Paths can be relative to the application path (./App_Data), absolute (/path/from/root), or fully qualified (D:\Path\To\App_Data). 
If the folder does not exist the application will attempt to create it.

### `IShellConfiguration` in the Global Tenant Configuration `App_Data/appsettings.json`

These settings can also be located in an `App_Data/appsettings.json` folder (not created by default), and any settings specified there will override settings from the `Startup` Project.

### `IShellConfiguration` in the Individual Tenants Folder

These settings are mutable and written during the setup for the Tenant. For this reason reading from Environment Name is not supported.
Additionally these `appsettings.json` files do not need the `OrchardCore` section

```json
{
  "OrchardCore_Media": {
    ... specific tenant configuration ...
  }
}
```

### `IShellConfiguration` via Environment Variables

Environment variables are also translated into `IShellConfiguration`, for example

```
OrchardCore__OrchardCore_Media__MaxFileSize

OrchardCore__Default__OrchardCore_Media__MaxFileSize

OrchardCore__MyTenant__OrchardCore_Media__MaxFileSize
```

!!! note
    To support Linux the underscore `_` is used as a separator, e.g. `OrchardCore_Media`
    `OrchardCore.Media` is supported for backwards compatibility, but users should migrate to the `_` pattern.

### Order of hierarchy

By default an Orchard Core site will use `CreateDefaultBuilder` in the Startup Project's `Program.cs` which will load `IConfiguration` in the following order

1. Startup project `appsettings.json`
2. Startup project `appsettings.{environment}.json`
3. User Secrets (if environment is __Development__)
4. Environment Variables
5. Command Line Args
6. `IShellConfiguration` will then add these
    1. `App_Data/appsettings.json`
    2. `App_Data/Sites/{tenant_name}/appsettings.json` for the particular tenant

!!! note
    Configurations with the same key that are loaded later take precedence over those which were loaded earlier (last wins).

### Configuration during Deployment

Azure App Settings are supported as Environment Variables on a Windows Environment, or a Linux Environment.

Azure DevOps, or other CI/CD pipelines, are supported, on all platforms, and Json Path Transformations can be used to transform `appsettings.json` files and provide app secrets from pipeline variables, or secret key stores like Azure Key Vault.

If building with the nightly dev builds from the preview package feed, the CI/CD pipeline will need to use a `NuGet.Config` with the location of the `MyGet` package feed.

```xml
<configuration>
  <packageSources>
    <add key="nuget" value="https://api.nuget.org/v3/index.json"/>
    <add key="preview" value="https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json" />
  </packageSources>
</configuration>
```

### Alternate locations

The `IShellConfiguration` values stored in the `App_Data` folder, and individual tenants `appsettings.json` files, can also be stored in alternate locations.

Refer to the [Shells Section](../Shells/README.md) for more details on this.
