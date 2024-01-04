using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Media",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Media",
    Name = "Media",
    Description = "The media module adds media management support.",
    Dependencies =
    [
        "OrchardCore.ContentTypes"
    ],
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Cache",
    Name = "Media Cache",
    Description = "The media cache module adds remote file store cache support.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Slugify",
    Name = "Media Slugify",
    Description = "The media slugify module transforms newly created folders and files into SEO-friendly versions by generating slugs.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Content Management"
)]
