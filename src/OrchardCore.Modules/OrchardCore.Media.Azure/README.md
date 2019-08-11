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
      "BasePath": "some/base/path", 
      // Access permissions for Azure Blob Storage. Valid options: Off, Container, Blob. Defaults to Blob.
      "PublicAccessType": "Blob",
      //"VersionHashCacheExpiryTime": 120 // If using Cache Busting, the time to expire the Blob ContentMD5 File Version Hash from memory cache, in minutes, defaults to 2 hours.
    
    },

```

## CREDITS

### ImageSharp

<https://sixlabors.com/projects/imagesharp/>

Copyright 2012 James South
Licensed under the Apache License, Version 2.0

### AspNetCore 

<https://github.com/aspnet/AspNetCore>

Copyright (c) .NET Foundation. All rights reserved.
Licensed under the Apache License, Version 2.0.