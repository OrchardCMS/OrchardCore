using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure Media",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Azure.Storage",
    Name = "Azure Media Storage",
    Description = "Enables support for storing media files in Microsoft Azure Blob Storage.",
    Dependencies =
    [
        "OrchardCore.Media.Cache"
    ],
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Azure.ImageCache",
    Name = "Azure Media Image Cache",
    Description = "Enables support for storing cached resized images in Microsoft Azure Blob Storage.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Hosting"
)]
