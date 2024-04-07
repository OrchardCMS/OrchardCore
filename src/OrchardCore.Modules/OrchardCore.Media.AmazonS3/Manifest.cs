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
    Description = "Enables support for storing media files in Amazon S3.",
    Dependencies =
    [
        "OrchardCore.Media.Cache"
    ],
    Category = "Hosting"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.AmazonS3.ImageSharpImageCache",
    Name = "Amazon Media ImageSharp Image Cache",
    Description = "Enables support for storing cached images resized via ImageSharp in Amazon S3.",
    Dependencies =
    [
        "OrchardCore.Media",
        "OrchardCore.Media.AmazonS3"
    ],
    Category = "Hosting"
)]
