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
