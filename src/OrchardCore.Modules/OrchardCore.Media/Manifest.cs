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

[assembly: Feature(
    Id = "OrchardCore.Media.Tus",
    Name = "Media TUS Uploads",
    Description = "Enables resumable file uploads using the TUS protocol. When enabled, replaces the default chunked upload mechanism with the TUS standard, allowing uploads to be paused and resumed.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.SignalR",
    Name = "Media SignalR",
    Description = "Enables real-time media updates via SignalR. When enabled, changes to media files and folders are broadcast to connected clients.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.SignalR.Azure",
    Name = "Media SignalR - Azure",
    Description = "Uses Azure SignalR Service as the backplane for real-time media updates, enabling multi-instance deployments.",
    Dependencies =
    [
        "OrchardCore.Media.SignalR"
    ],
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.SignalR.Redis",
    Name = "Media SignalR - Redis",
    Description = "Uses Redis as the backplane for real-time media updates, enabling multi-instance deployments.",
    Dependencies =
    [
        "OrchardCore.Media.SignalR"
    ],
    Category = "Content Management"
)]
