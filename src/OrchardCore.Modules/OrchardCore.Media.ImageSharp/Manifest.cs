using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Media ImageSharp",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Media.ImageSharp",
    Name = "Media ImageSharp Image Processing",
    Description = "Provides the default media image processing engine, based on ImageSharp.",
    Category = "Content Management",
    EnabledByDependencyOnly = true
)]
