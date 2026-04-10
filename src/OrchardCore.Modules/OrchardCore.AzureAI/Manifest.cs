using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure AI Search",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.AzureAI",
    Name = "Azure AI Search",
    Description = "Provides Azure AI Search services for managing indexes and facilitating search scenarios within indexes.",
    Dependencies =
    [
        "OrchardCore.Indexing",
    ],
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Search.AzureAI",
    Name = "Azure AI Search (Obsolete)",
    Description = "Obsolete legacy feature ID kept for backwards compatibility. Enables OrchardCore.AzureAI automatically.",
    Dependencies =
    [
        "OrchardCore.AzureAI",
    ],
    Category = "Search"
)]
