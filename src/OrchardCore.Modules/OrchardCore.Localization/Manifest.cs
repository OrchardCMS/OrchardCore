using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Localization",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Localization",
    Name = "Localization",
    Description = "Provides support for UI localization.",
    Dependencies = new[] { "OrchardCore.Settings" },
    Category = "Internationalization"
)]

[assembly: Feature(
    Id = "OrchardCore.Localization.ContentLanguageHeader",
    Name = "Content Language Header",
    Description = "Adds the Content-Language HTTP header, which describes the language(s) intended for the audience.",
    Dependencies = new[] { "OrchardCore.Localization" },
    Category = "Internationalization"
)]
