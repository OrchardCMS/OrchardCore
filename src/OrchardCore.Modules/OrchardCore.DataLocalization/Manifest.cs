using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Data Localization",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides support for data localization.",
    Category = "Internationalization"
)]

[assembly: Feature(
    Id = "OrchardCore.DataLocalization",
    Name = "Data Localization",
    Description = "Provides support for data localization.",
    Category = "Internationalization",
    Dependencies = ["OrchardCore.Localization"]
)]

[assembly: Feature(
    Id = "OrchardCore.DataLocalization.Ui",
    Name = "UI Localization Overrides",
    Description = "Manages database UI translations from embedded localization catalogs.",
    Category = "Internationalization",
    Dependencies = ["OrchardCore.DataLocalization"]
)]
