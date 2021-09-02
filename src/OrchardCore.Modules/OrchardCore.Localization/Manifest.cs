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
#if (NET5_0 || NET5_0_OR_GREATER)
[assembly: Feature(
    Id = "OrchardCore.Localization.ContentLanguageHeader",
    Name = "Content Language Header",
    Description = "Adds the Content-Language HTTP header, which describes the language(s) intended for the audience.",
    Category = "Internationalization"
)]
#endif
