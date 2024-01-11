using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Settings",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The settings module creates site settings that other modules can contribute to.",
    Category = "Configuration",
    IsAlwaysEnabled = true
)]
