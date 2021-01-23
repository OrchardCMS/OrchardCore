using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Dynamic Localization",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides support for dynamic data localization.",
    Category = "Internationalization",
    Dependencies = new[] { "OrchardCore.Localization" }
)]
