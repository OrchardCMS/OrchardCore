using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Elasticsearch",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Elasticsearch",
    Name = "Elasticsearch",
    Description = "Creates Elasticsearch indexes to support search scenarios, introduces a preconfigured container-enabled content type.",
    Dependencies = new[]
    {
        "OrchardCore.Indexing",
        "OrchardCore.ContentTypes"
    },
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Elasticsearch.Worker",
    Name = "Elasticsearch Worker",
    Description = "Provides a background task to keep indices in sync with other instances.",
    Dependencies = new[] { "OrchardCore.Search.Elasticsearch" },
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Elasticsearch.ContentPicker",
    Name = "Elasticsearch Content Picker",
    Description = "Provides a Elasticsearch content picker field editor.",
    Dependencies = new[] { "OrchardCore.Search.Elasticsearch", "OrchardCore.ContentFields" },
    Category = "Search"
)]
