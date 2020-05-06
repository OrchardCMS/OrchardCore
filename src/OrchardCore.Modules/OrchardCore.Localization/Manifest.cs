using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Localization",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides support for UI localization.",
    Category = "Internationalization",
    Dependencies = new[] { "OrchardCore.Settings" }
)]
