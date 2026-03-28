using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenSearch",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Search.OpenSearch",
    Name = "OpenSearch",
    Description = "Creates OpenSearch indexes to support search scenarios, introduces a preconfigured container-enabled content type.",
    Dependencies =
    [
        "OrchardCore.Queries.Core",
        "OrchardCore.Indexing",
        "OrchardCore.ContentTypes",
    ],
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.OpenSearch.Worker",
    Name = "OpenSearch Worker",
    Description = "Provides a background task to keep indices in sync with other instances.",
    Dependencies = ["OrchardCore.Search.OpenSearch"],
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.OpenSearch.ContentPicker",
    Name = "OpenSearch Content Picker",
    Description = "Provides an OpenSearch content picker field editor.",
    Dependencies = ["OrchardCore.Search.OpenSearch", "OrchardCore.ContentFields"],
    Category = "Search"
)]
