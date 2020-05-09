using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Themes",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Themes.",
    Dependencies = new[] { "OrchardCore.Admin" },
    Category = "Theming"
)]
