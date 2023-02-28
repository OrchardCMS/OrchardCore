using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lucene",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Lucene",
    Name = "Lucene",
    Description = "Creates Lucene indexes to support search scenarios, introduces a preconfigured container-enabled content type.",
    Dependencies = new[]
    {
        "OrchardCore.Indexing",
        "OrchardCore.ContentTypes"
    },
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Lucene.Worker",
    Name = "Lucene Worker",
    Description = "Provides a background task to keep local indices in sync with other instances.",
    Dependencies = new[] { "OrchardCore.Search.Lucene" },
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Lucene.ContentPicker",
    Name = "Lucene Content Picker",
    Description = "Provides a Lucene content picker field editor.",
    Dependencies = new[] { "OrchardCore.Search.Lucene", "OrchardCore.ContentFields" },
    Category = "Search"
)]
