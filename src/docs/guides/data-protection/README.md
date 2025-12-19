# Data Protection in Orchard Core

This guide provides comprehensive information about implementing data protection in Orchard Core applications, particularly in multi-tenant and load-balanced environments.

## Overview

Data Protection is a critical security feature in ASP.NET Core that Orchard Core leverages to protect sensitive data such as authentication cookies, anti-forgery tokens, persisted secrets that need to be decrypted (e.g. SMTP passwords but not user passwords), and temporary data. In a multi-tenant or load-balanced environment, it's essential to ensure that data protection keys are shared across all nodes and persisted properly.

Orchard Core provides several options for persisting data protection keys:

- Local storage in the `App_Data` folder's tenant-specific folder (like `App_Data/Sites/Default/DataProtection-Keys`)
- [Azure Blob Storage](#azure-blob-storage-data-protection)
- [Redis](#redis-data-protection)
- 

This guide will focus on the distributed options, since the local storage one just works out of the box without any configuration.

## Why Distributed Data Protection Matters

In a single-server deployment, data protection keys are stored locally by default. However, this approach creates problems in:

1. **Load-balanced environments**: Each server has its own keys, causing authentication failures when requests are routed to different servers
2. **Multi-tenant setups**: Each tenant needs isolated but persistent key storage
3. **Application restarts**: Locally stored keys may be lost, invalidating existing cookies and tokens

Distributed data protection solves these issues by storing keys in one or more shared locations accessible to all application instances.

## Azure Blob Storage Data Protection

The `OrchardCore.DataProtection.Azure` module enables storing data protection keys in Azure Blob Storage, providing a reliable and scalable solution for distributed applications.

### Purpose

Azure Blob Storage data protection enables key rings that are segregated by tenant and stored in an Azure Blob Storage container. This is particularly useful for load-balanced environments where each active node needs to share the same key ring.

### Configuration

To configure Azure Blob Storage for data protection, you need to specify a storage account connection string and a valid container name. The container will be automatically created if it doesn't already exist.

Add the following configuration to your `appsettings.json` file:

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

#### Configuration Options

- **ConnectionString**: The Azure Storage account connection string (required)
- **ContainerName**: The name of the Azure Blob container (defaults to "dataprotection")
- **BlobName**: The specific blob name for storing keys (optional, defaults to tenant-specific path)
- **CreateContainer**: Whether to automatically create the container if it doesn't exist (defaults to true)

### Key Storage Structure

By default, this configuration uses a single container to store all Data Protection Keys based on a folder per tenant configuration:

```
dataprotection/Sites/tenant_name/DataProtectionKeys.xml
```

During startup, if `CreateContainer` is set to `true`, Data Protection will check if the container exists and create it if it doesn't. Set `CreateContainer` to `false` to disable this check if your container already exists.

### Advanced Configuration with Liquid Templating

You can use Liquid templating to further configure Data Protection. The `ShellSettings` property is made available to the liquid template. The `ContainerName` and `BlobName` properties are the only templatable properties.

If not supplied, the `BlobName` will automatically default to a folder per tenant configuration: `Sites/tenant_name/DataProtectionKeys.xml`

#### Configuring a Container per Tenant

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
    When templating the `ContainerName` using `{{ ShellSettings.Name }}`, the tenant's name will be automatically lowercased. However, you must also ensure the `ContainerName` conforms to other Azure Blob naming conventions as outlined in Azure's documentation.

!!! important
    Only the default Liquid filters and tags are available during parsing of the Liquid template. Extra filters like `slugify` will not be available.

## Redis Data Protection

The `OrchardCore.Redis.DataProtection` feature enables storing data protection keys in Redis, providing a high-performance solution for distributed applications.

### Purpose

Redis Data Protection enables sharing data protection keys across multiple application instances using Redis as the centralized storage. This is ideal for load-balanced environments where low-latency key access is required.

### Prerequisites

Before configuring Redis Data Protection, ensure you have:

1. A running Redis instance
2. The `OrchardCore.Redis` module enabled
3. The `OrchardCore.Redis.DataProtection` feature enabled

### Configuration

First, configure the basic Redis connection in your `appsettings.json`:

```json
{
  "OrchardCore": {
    "OrchardCore_Redis": {
      "Configuration": "<your-redis-connection-string>",
      "InstancePrefix": "MyApp:",
      "AllowAdmin": true
    }
  }
}
```

#### Configuration Options

- **Configuration**: The Redis connection string (required)
- **InstancePrefix**: A prefix to be added to all Redis keys (optional)
- **AllowAdmin**: Whether to allow admin commands (optional, defaults to false)

### Key Storage Structure

Redis Data Protection stores keys using the following pattern:

```
{InstancePrefix}{TenantName}:DataProtection-Keys
```

For example, with an InstancePrefix of "MyApp:" and a tenant named "Default", the key would be:

```
MyApp:Default:DataProtection-Keys
```

### Persistence Considerations

!!! warning
    Data protection keyrings are not cache files and must be kept in durable storage. Ensure that your Redis server has a backup strategy in place to prevent data loss. Use either AOF (Append-Only File) or RDB (Redis Database) persistence.

For more details on Redis persistence, visit the [Redis documentation](https://redis.io/docs/latest/operate/oss_and_stack/management/persistence/).

The Redis Data Protection module will automatically check if persistence is enabled (when `AllowAdmin` is true) and log a warning if it's not configured.

For more information about specific implementations, refer to the module documentation:

- [Data Protection (Azure Storage)](../../reference/modules/DataProtection.Azure/README.md)
- [Redis](../../reference/modules/Redis/README.md)