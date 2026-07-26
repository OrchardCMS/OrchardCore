using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Media ImageSharp",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Media.ImageSharpV3",
    Name = "Media ImageSharp Image Processing",
    Description = "Replaces the default media image processing engine with an ImageSharp (v3) based implementation.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Content Management"
)]
