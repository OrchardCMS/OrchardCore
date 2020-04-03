# Azure (`OrchardCore.Azure`)

## Configuration

The following configuration values are used by default and can be customized:

```json
    "OrchardCore.Media.Azure":
    {
      // Set to your Azure Storage account connection string.
      "ConnectionString": "", 
      // Set to the Azure Blob container name.
      "ContainerName": "somecontainer",
      // Optionally, set to a path to store media in a subdirectory inside your container.
      "BasePath": "some/base/path"
    },

```
