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
    Description = "The Placements module provides a way to define shape placement in admin UI.",
    Category = "Development"
)]

[assembly: Feature(
    Id = "OrchardCore.Placements.FileStorage",
    Name = "Placements file storage",
    Description = "Stores Placements in a local file.",
    Dependencies = new[] { "OrchardCore.Placements" },
    Category = "Development"
)]
