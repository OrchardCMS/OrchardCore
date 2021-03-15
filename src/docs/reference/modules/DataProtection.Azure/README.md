# Data Protection (Azure Storage) (OrchardCore.DataProtection.Azure)

## Purpose

Data Protection (Azure Storage) enables data protection key rings that are by default segregated by tenant and stored in an Azure Blob Storage container.  
This is useful for load balanced environments where each active node will need to share the same key ring.

## Configuration

You'll need to specify a storage account connection string and a valid container name. The container will automatically be created if it does not already exist.

These settings need to be available to the `IShellConfiguration` implementation. In the simplest case, this will mean updating your `appsettings.json` file:

```json
{
  "OrchardCore": {
    "OrchardCore.DataProtection.Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccountname>;AccountKey=<myaccountkey>;EndpointSuffix=core.windows.net",
      "ContainerName": "dataprotection",
      "BlobName": "",
      "CreateContainer": true
    }
  }
}
```

By default this will use a single container to store all the Data Protection Keys based on a folder per tenant configuration.

`dataprotection/Sites/tenant_name/DataProtectionKeys.xml`

During `Startup` if `CreateContainer` is set to true, Data Protection will check the container exists, and create it, if it does not.
Set `CreateContainer` to `false` to disable this check if your container already exists.

## Templating Configuration

Optionally you may use liquid templating to further configure Data Protection.
The `ShellSettings` property is made available to the liquid template.
The `ContainerName` property and the `BlobName` property are the only templatable properties.
If not supplied the `BlobName` will automatically default to a folder per tenant configuration, i.e. `Sites/tenant_name/DataProtectionKeys.xml`

### Configuring a container per tenant.

```json
{
  "OrchardCore": {
    "OrchardCore.DataProtection.Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccountname>;AccountKey=<myaccountkey>;EndpointSuffix=core.windows.net",
      "ContainerName": "{{ ShellSettings.Name }}-dataprotection",
      "BlobName": "{{ ShellSettings.Name }}DataProtectionKeys.xml",
      "CreateContainer": true
    }
  }
}
```

!!! note
    Only the default liquid filters and tags are available during parsing of the liquid template.
    Extra filters like `slugify` will not be available.

Refer also to the [Configuration Section](../../core/Configuration/README.md).
