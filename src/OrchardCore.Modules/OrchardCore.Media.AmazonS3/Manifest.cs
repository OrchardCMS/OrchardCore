using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Amazon S3 Media",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Media.AmazonS3",
    Name = "Amazon Media Storage",
    Description = "Enables support for storing media files in Amazon S3 Bucket.",
    Dependencies = new[]
    {
        "OrchardCore.Media.Cache"
    },
    Category = "Hosting"
)]
