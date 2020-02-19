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
    Description = "Enables support for storing media files in Microsoft Azure Blob Storage.",
    Dependencies = new[]
    {
        "OrchardCore.Media.Cache"
    },
    Category = "Hosting"
)]
