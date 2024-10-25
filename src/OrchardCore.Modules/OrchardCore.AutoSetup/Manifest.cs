using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Auto Setup",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The auto setup module allows to automatically install the application / tenants",
    Dependencies = ["OrchardCore.Setup"],
    Category = "Infrastructure"
)]
