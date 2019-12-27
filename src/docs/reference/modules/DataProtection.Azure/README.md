# Data Protection (Azure Storage) (OrchardCore.DataProtection.Azure)

## Purpose

Data Protection (Azure Storage) enables data protection key rings that are by default segregated by tenant and stored in an Azure Blob Storage container.  
This is useful for load balanced environments where each active node will need to share the same key ring.

## Configuration

You'll need to specify a storage account connection string and a valid container name. The container will automatically be created if it does not already exist.

These settings need to be available to the `IConfiguration` implementation. In the simplest case, this will mean updating your `appsettings.json` file:

```json
{
  "OrchardCore": {
    "OrchardCore.DataProtection.Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccountname>;AccountKey=<myaccountkey>;EndpointSuffix=core.windows.net",
      "ContainerName": "dataprotection"
    }
  }
}
```

By default this will use a single container to store all the Data Protection Keys based on a folder per tenant configuration.

`dataprotection/Sites/tenant_name/DataProtectionKeys.xml`

## Templating Configuration

Optionally you may use liquid templating to further configure Data Protection, perhaps creating a container per tenant.
The `ShellSettings` property is made available to the liquid template.
The `ContainerName` property and the `BlobName` property are the only templatable properties.
If not supplied the `BlobName` will automatically default to a folder per tenant configuration, i.e. `dataprotection/Sites/tenant_name/DataProtectionKeys.xml`

```json
{
  "OrchardCore": {
    "OrchardCore.DataProtection.Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccountname>;AccountKey=<myaccountkey>;EndpointSuffix=core.windows.net",
        "ContainerName": "{{ ShellSettings.Name }}-dataprotection",
        "BlobName": "{{ ShellSettings.Name }}/DataProtectionKeys.xml"
      }
  }
}
```

!!! note
Only the default liquid filters and tags are available during parsing of the liquid template.
Extra filters like `slugify` will not be available.


Refer also to the [Configuration Section](../../core/Configuration/README.md).