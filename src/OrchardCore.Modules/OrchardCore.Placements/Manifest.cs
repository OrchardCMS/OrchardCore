using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Placements",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Placements",
    Name = "Placements",
    Description = "The Placements module provides a way to define shape placement.",
    Category = "Development"
)]
