using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure Cognitive Search",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Azure.CognitiveSearch",
    Name = "Azure Cognitive Search",
    Description = "Creates Azure Cognitive Search indexes to support search scenarios, introduces a preconfigured container-enabled content type.",
    Dependencies =
    [
        "OrchardCore.Indexing",
    ],
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.Azure.CognitiveSearch.Worker",
    Name = "Azure Cognitive Search Worker",
    Description = "Provides a background task to keep indices in sync with other instances.",
    Dependencies =
    [
        "OrchardCore.Search.Elasticsearch",
    ],
    Category = "Search"
)]
