using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Localization",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides a part that allows to localize content items.",
    Category = "Internationalization"
)]

[assembly: Feature(
    Id = "OrchardCore.ContentLocalization",
    Name = "Content Localization",
    Description = "Provides a part that allows to localize content items.",
    Dependencies = new[] { "OrchardCore.ContentTypes", "OrchardCore.Localization" },
    Category = "Internationalization"
)]

[assembly: Feature(
    Id = "OrchardCore.ContentLocalization.ContentCulturePicker",
    Name = "Content Culture Picker",
    Description = "Provides a culture picker shape for the frontend.",
    Dependencies = new[] { "OrchardCore.ContentLocalization", "OrchardCore.Autoroute" },
    Category = "Internationalization"
)]

[assembly: Feature(
    Id = "OrchardCore.ContentLocalization.Sitemaps",
    Name = "Localized Content Item Sitemaps",
    Description = "Provides support for localized content item sitemaps.",
    Dependencies = new[] { "OrchardCore.ContentLocalization", "OrchardCore.Sitemaps" },
    Category = "Internationalization"
)]
