# Shells Configuration Providers

The Azure Shells Configuration and Database Shells Configuration providers allow hosting of shell / tenant 
`tenants.json` files and related tenants `appsettings.json` configuration settings in an environment external to the Orchard Core host.

By default the `tenants.json` and related `appsettings.json` for the `Default` shell and any configured tenants
are stored in the `App_Data` folder.

This configuration in the `App_Data` folder is suitable for most sites, however for advanced configuration
of stateless multi-tenancy environments, where multiple hosts require write access to these shared configuration settings,
you can choose to use either the Azure Shells Configuration or Database Shells Configuration providers.

The primary purpose of the Shell Configuration providers is to provide a shared external environment for multi-tenancy
where tenants need to be created, and their settings mutated, during live operation of the stateless hosts.

It is not intended to support shared configuration between local development and production environments.

## Azure Shells Configuration Provider

The Azure Shells Configuration provider uses an Azure Blob Storage Container to store the `tenants.json` and related tenant `appsettings.json` 
files in a similar hierarchy to that of the default `App_Data` configuration.

The root of the Azure Blob Container includes a `tenants.json` file, and optionally can include a `appsettings.json` file.

Each shell, or tenant has a directory under the `Sites` folder, named for the tenant, with an individual `appsettings.json` file.

The hierarchy is separated into single files, and is useful if you need to manage the tenants `appsettings.json` independently from Orchard Core.
For example, you may prefer to provide different Azure Blob Storage keys, for each tenant when using the Azure Media Storage feature.

The Azure Shells Configuration supports a root `appsettings.json` and `appsettings.Environment.json` file.

!!! note
    Individual tenants do not support a `appsettings.Environment.json` file.

### Enable Azure Shells Configuration

The Azure Shells Configuration is provided by a separate NuGet package: `OrchardCore.Shells.Azure`

The Azure Shells Configuration is configured via the `appsettings.json` section in the web host project.

``` json
{
  "OrchardCore": {
    "OrchardCore_Shells_Azure": {
      "ConnectionString": "", // Set to your Azure Storage account connection string.
      "ContainerName": "hostcontainer", // Set to the Azure Blob container name.
      "BasePath": "some/base/path", // Optionally, set to a subdirectory inside your container.
      "MigrateFromFiles": true // Optionally, enable to migrate existing App_Data files to Blob automatically.
    }
  }
}
```

In the web host `Startup.cs` it is enabled via an extension method on the Orchard Core Builder.

``` csharp
namespace OrchardCore.Cms.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms()
                .AddAzureShellsConfiguration();
        }
    }
}
```

!!! note
    The container must be created before using the Azure Shells Configuration provider.
    Make sure this container is secure and access to keys is limited.

## Database Shells Configuration Provider

The Database Shells Configuration provider uses any supported database to store all the tenant related configuration
as part of a single json document.

The Database Shells Configuration provider does not support a site `appsettings.json` and `appsettings.Environment.json` file.

### Enable Database Shells Configuration

The Database Shells Configuration is configured via the `appsettings.json` section in the web host project.

``` json
{
  "OrchardCore": {
    "OrchardCore_Shells_Database": {
      "DatabaseProvider": "SqlConnection", // Set to a supported database provider.
      "ConnectionString": "", // Set to the database connection string.
      "TablePrefix": "", // Optionally, configure a table prefix.
      "MigrateFromFiles": true // Optionally, enable to migrate existing App_Data files to Database automatically.
    },
  }
}
```

In the web host `Startup.cs` it is enabled via an extension method on the Orchard Core Builder.

``` csharp
namespace OrchardCore.Cms.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms()
                .AddDatabaseShellsConfiguration();
        }
    }
}
```

!!! note
    The database must be created before using the Database Configuration provider.
    Make sure this database is secure and access to it is limited.

## Migrate From Files

The `MigrateFromFiles` option is available for both the Azure Shells and Database Shells Configuration providers
to assist migrating from an existing `App_Data` configuration.

When enabled the Shell Configuration provider will first check to see if a configuration exists for a given tenant
for the chosen storage platform, Database, or Azure Blob Storage.

If the configuration does not exist, the provider will try to load it from the `App_Data` folder, 
and migrate it to the storage platform of choice.

## Environment Options

To disable a provider in Development, or different environments, inject the `IHostEnvironment` 

``` csharp
namespace OrchardCore.Cms.Web
{
    public class Startup
    {
        private readonly IHostEnvironment _env;

        public Startup(IHostEnvironment env)
        {
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddOrchardCms();
            if (!_env.IsDevelopment())
            {
                builder.AddDatabaseShellsConfiguration();
            }
        }
    }
}
```
