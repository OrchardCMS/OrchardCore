using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Setup",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The setup module is creating the application's setup experience.",
    Dependencies = new[] { "OrchardCore.Recipes" },
    Category = "Infrastructure"
)]
