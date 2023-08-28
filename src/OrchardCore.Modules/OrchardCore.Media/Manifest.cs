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
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Cache",
    Name = "Media Cache",
    Description = "The media cache module adds remote file store cache support.",
    Dependencies = new[]
    {
        "OrchardCore.Media"
    },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Slugify",
    Name = "Media Slugify",
    Description = "The media slugify module slugifies new folders and files to make them SEO-friendly.",
    Dependencies = new[]
    {
        "OrchardCore.Media"
    },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Indexing",
    Name = "Media Indexing",
    Description = "Provides a way to index media files with common format in Lucene and Elasticsearch.",
    Dependencies = new[]
    {
        "OrchardCore.Media"
    },
    Category = "Search"
)]
