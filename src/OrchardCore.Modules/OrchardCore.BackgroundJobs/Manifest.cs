using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Background Jobs",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Infrastructure"
)]

[assembly: Feature(
    Id = "OrchardCore.BackgroundJobs",
    Name = "Background Jobs",
    Description = "This feature provides tools to manage background jobs.",
    Category = "Infrastructure"
)]

