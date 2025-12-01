using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Search",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Search",
    Name = "Search",
    Description = "Provides frontend search capabilities against indexes.",
    Category = "Search",
    Dependencies =
    [
        "OrchardCore.Indexing",
    ]
)]
