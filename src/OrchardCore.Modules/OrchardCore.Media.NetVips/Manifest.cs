using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Media NetVips",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Media.NetVips",
    Name = "Media NetVips Image Processing",
    Description = "Replaces the default media image processing engine with a NetVips based implementation, using the native libvips library.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Content Management"
)]
