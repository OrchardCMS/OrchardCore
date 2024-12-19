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
    Id = "OrchardCore.Media.Indexing",
    Name = "Media Indexing",
    Description = "Provides a way to index media files with common format in search providers.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Search",
    EnabledByDependencyOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Indexing.Text",
    Name = "Text Media Indexing",
    Description = "Provides a way to index common text files like (.txt and .md) in search providers.",
    Dependencies =
    [
        "OrchardCore.Media.Indexing"
    ],
    Category = "Search"
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

[assembly: Feature(
    Id = "OrchardCore.Media.Security",
    Name = "Secure Media",
    Description = "Adds permissions to restrict access to media folders.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Content Management"
)]
