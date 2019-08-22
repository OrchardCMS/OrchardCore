using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Microsoft Azure Media",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Azure.Storage",
    Name = "Azure Media Storage",
    Description = "Enables support for storing media files in Microsoft Azure Blob Storage, and delivering them via cache from this site.",
    Dependencies = new[] { "OrchardCore.Media" },
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Azure.MediaCache",
    Name = "Azure Media Storage Cache Management",
    Description = "Enables management of the Media Cache used to cache files retrieved from Microsoft Azure Blob Storage.",
    Dependencies = new[] { "OrchardCore.Media.Azure.Storage", "OrchardCore.Media.MediaCache" },
    Category = "Hosting"
)]
