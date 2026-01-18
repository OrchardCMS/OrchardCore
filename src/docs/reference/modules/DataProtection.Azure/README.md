# Data Protection (Azure Storage) (`OrchardCore.DataProtection.Azure`)

## Purpose

The Data Protection (Azure Storage) feature allows data protection key rings to be stored in an Azure Blob Storage container, with keys automatically isolated per tenant. This is particularly important in load-balanced deployments, where all active instances must access the same key ring to operate correctly.

Data Protection is a core security mechanism in ASP.NET Core and is used by Orchard Core to safeguard sensitive information, including authentication cookies, anti-forgery tokens, decryptable persisted secrets (such as SMTP credentials, excluding user passwords), and temporary data. In multi-tenant or distributed environments, sharing and reliably persisting data protection keys across all nodes is essential to maintain security and system consistency.

## Configuration

You'll need to specify a storage account connection string and a valid container name. The container will automatically be created if it does not already exist.

These settings need to be available to the `IShellConfiguration` implementation. In the simplest case, this will mean updating your `appsettings.json` file:

```json
{
    "OrchardCore": {
        "OrchardCore_DataProtection_Azure": {
            "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccountname>;AccountKey=<myaccountkey>;EndpointSuffix=core.windows.net",
            "ContainerName": "dataprotection",
            "BlobName": "",
            "CreateContainer": true
        }
    }
}
```

### Configuration Options

- **ConnectionString**: The Azure Storage account connection string (required)
- **ContainerName**: The name of the Azure Blob container (defaults to "dataprotection"). It must be a valid DNS name and conform to Azure container naming rules, e.g. lowercase only.
- **BlobName**: The specific blob name for storing keys (optional, defaults to tenant-specific path)
- **CreateContainer**: Whether to automatically create the container if it doesn't exist (defaults to true)

By default this will use a single container to store all Data Protection Keys based on a folder per tenant configuration:

```
dataprotection/Sites/tenant_name/DataProtectionKeys.xml
```

During `Startup` if `CreateContainer` is set to `true`, Data Protection will check if the container exists and create it if it doesn't. Set `CreateContainer` to `false` to disable this check if your container already exists.

## Advanced Configuration with Liquid Templating

Optionally you may use Liquid templating to further configure Data Protection.
The `ShellSettings` property is made available to the Liquid template.
The `ContainerName` property and the `BlobName` property are the only templatable properties.
If not supplied the `BlobName` will automatically default to a folder per tenant configuration, i.e. `Sites/tenant_name/DataProtectionKeys.xml`

!!! note
When templating the `ContainerName` using `{{ ShellSettings.Name }}`, the tenant's name will be automatically lowercased. However, you must also make sure the `ContainerName` conforms to other Azure Blob naming conventions as set out in Azure's documentation.

### Configuring a container per tenant

For more granular isolation, you can configure a separate container for each tenant:

```json
{
    "OrchardCore": {
        "OrchardCore_DataProtection_Azure": {
            "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccountname>;AccountKey=<myaccountkey>;EndpointSuffix=core.windows.net",
            "ContainerName": "{{ ShellSettings.Name }}-dataprotection",
            "BlobName": "{{ ShellSettings.Name }}DataProtectionKeys.xml",
            "CreateContainer": true
        }
    }
}
```

!!! note
Only the default Liquid filters and tags are available during parsing of the Liquid template.
Extra filters like `slugify` will not be available.

Refer also to the [Configuration Section](../Configuration/README.md).
