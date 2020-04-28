# Configuration

Orchard Core extends ASP.NET Core `IConfiguration` with `IShellConfiguration`.

To learn more about ASP.NET Core `IConfiguration` visit <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration>

## Config Sources

Orchard Core supports a hierarchy of Configuration Sources

* The `Startup` ASP.NET Core Project, e.g. `OrchardCore.Cms.Web.csproj`, `appsettings.json`, or by environment  `appsettings.Development.json`
* Global Tenant Configuration `App_Data/appsettings.json`, or by environment `App_Data/appsettings.Development.json`  
* Individual Tenant Configuration files located under each Tenant Folder in the `App_Data/Sites/{tenant_name}/appsettings.json` folder. **Note:** These are mutable files, and do not support an Environment version
* Environment Variables, or AppSettings as Environment Variables via Azure

The Configuration Sources are loaded in the above order, and settings lower in the hierarchy will override values configured higher up, i.e. an Global Tenant value will always be overridden by an Environment Variable.

### `IShellConfiguration` in the `OrchardCore.Cms.Web.csproj` Startup Project

Orchard Core stores all Configuration data under the `OrchardCore` section in `appsettings.json` files

```
{
  "OrchardCore": {
      ... module configurations ...
  }
}
```

Each Orchard Core module has its own configuration section under the `OrchardCore` section

```
{
  "OrchardCore": {
    "OrchardCore.Media": {
      ... individual module configuration ...
    }
  }
}
```

### Tenant Postconfiguration

To configure the values for a tenant after it has been created you can specify a section named for the tenant.

```
{
  "OrchardCore": {
    "Default": {
      "OrchardCore_Media": {
        ... specific tenant configuration configuration ...
      }
    }
  }
}
```

### `ORCHARD_APP_DATA` Environment Variable

The location of the `App_Data` folder can be configured by setting the `ORCHARD_APP_DATA` environment variable. Paths can be relative to the application path (./App_Data), absolute (/path/from/root), or fully qualified (D:\Path\To\App_Data). If the folder does not exist the application will attempt to create it.

### `IShellConfiguration` in the Global Tenant Configuration `App_Data/appsettings.json`

These settings can also be located in an `App_Data/appsettings.json` folder (not created by default), and any settings specified there will override settings from the `Startup` Project.

### `IShellConfiguration` in the Individual Tenants Folder

These settings are mutable and written during the setup for the Tenant. For this reason reading from Environment Name is not supported.
Additionally these `appsettings.json` files do not need the `OrchardCore` section

```
{
  "OrchardCore_Media": {
    ... specific tenant configuration configuration ...
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
    To support Linux the underscore `_` is used as a seperator, e.g. `OrchardCore_Media`
    `OrchardCore.Media` is supported for backwards compatability, but users should migrate to the `_` pattern.

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

If building with the nightly dev builds from the `MyGet` package feed, the CI/CD pipeline will need to use a `NuGet.Config` with the location of the `MyGet` package feed.

```
<configuration>
  <packageSources>
    <add key="nuget" value="https://api.nuget.org/v3/index.json"/>
    <add key="myget" value="https://www.myget.org/F/orchardcore-preview/api/v3/index.json" />
  </packageSources>
</configuration>
```
