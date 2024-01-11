using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Themes",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The Themes modules allows you to specify the Front and the Admin theme.",
    Dependencies = new[] { "OrchardCore.Admin" },
    Category = "Theming"
)]
