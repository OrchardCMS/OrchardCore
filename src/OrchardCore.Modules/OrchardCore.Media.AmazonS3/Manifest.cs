using OrchardCore.Modules.Manifest;

[assembly: Module(
    Id = "OrchardCore.Media.AmazonS3.Storage",
    Name = "Amazon S3 Media Storage",
    Description = "Enables support for storing media files in Amazon S3 Bucket.",
    Dependencies = new[]
    {
        "OrchardCore.Media.Cache"
    },
    Category = "Hosting",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

