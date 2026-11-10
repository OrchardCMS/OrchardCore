using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Media ImageSharp",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

// The services and middleware are added by the OrchardCore.Media module, through the extension
// methods of this assembly, so that the middleware stays at its position in the media pipeline.
[assembly: Feature(
    Id = "OrchardCore.Media.ImageSharp",
    Name = "Media ImageSharp Image Processing",
    Description = "Provides the media image processing, based on ImageSharp.",
    Category = "Content Management",
    EnabledByDependencyOnly = true
)]
