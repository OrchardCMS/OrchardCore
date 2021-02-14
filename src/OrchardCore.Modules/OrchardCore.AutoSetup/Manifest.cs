using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Auto Setup",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The auto setup module allows automatically install the application on the first start",
    Dependencies = new[] { "OrchardCore.Setup" },
    Category = "Infrastructure"
)]
