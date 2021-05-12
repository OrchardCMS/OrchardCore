using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Admin",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Creates an admin section for the site.",
    Category = "Infrastructure",
    Dependencies = new[]
    {
        "OrchardCore.Settings"
    }
)]
