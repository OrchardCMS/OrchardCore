# Data Protection (Azure Storage) (OrchardCore.DataProtection.Azure)

## Purpose

Data Protection (Azure Storage) enables data protection key rings that are segregated by tenant and stored in an Azure Blob Storage container. 
This is useful for load balanced environments where each active node will need to share the same key ring.

## Configuration

You'll need to specify a storage account connection string and a valid container name. The container will automatically be created if it does not already exist.

These settings need to be available to the `IConfiguration` implementation. In the simplest case, this will mean updating your `appsettings.json` file:

```
{
    "Modules": {
        "OrchardCore.DataProtection.Azure": {
          "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccountname>;AccountKey=<myaccountkey>;EndpointSuffix=core.windows.net",
          "ContainerName": "dataprotection"
        }
    }
}
```
