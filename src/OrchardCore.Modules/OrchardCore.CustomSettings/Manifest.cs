using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Custom Settings",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The custom settings modules enables content types to become custom site settings.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Settings"
)]
