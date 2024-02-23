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
    Description = "The Search module adds frontend search capabilities.",
    Category = "Search"
)]
