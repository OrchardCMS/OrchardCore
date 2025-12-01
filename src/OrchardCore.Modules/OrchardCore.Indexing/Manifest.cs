using OrchardCore.Indexing.Core;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Indexing",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Name = "Indexing",
    Id = IndexingConstants.Feature.Area,
    Description = "Provides index management.",
    Category = "Indexing"
)]

[assembly: Feature(
    Name = "Indexing Worker",
    Id = IndexingConstants.Feature.Worker,
    Description = "Provides a background task to keep indexes in sync with the latest content item update",
    Category = "Indexing",
    Dependencies = [IndexingConstants.Feature.Area, "OrchardCore.Contents"]
)]
