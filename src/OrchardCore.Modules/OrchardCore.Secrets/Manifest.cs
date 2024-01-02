using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Secrets",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]


[assembly: Feature(
    Id = "OrchardCore.Secrets",
    Name = "Secrets",
    Description = "The secrets feature manages secrets that other modules can access and contribute to.",
    Category = "Configuration"
)]
