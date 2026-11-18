using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Themes",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Theming"
)]

[assembly: Feature(
    Id = "OrchardCore.Themes",
    Name = "Themes",
    Description = "The Themes modules allows you to specify the Front and the Admin theme.",
    Dependencies = ["OrchardCore.Admin"],
    Category = "Theming"
)]

[assembly: Feature(
    Id = "OrchardCore.Themes.Schemes",
    Name = "Theme Schemes",
    Description = "Lets a theme that supports it offer several schemes (colors, typefaces, size, density and corners) that admins manage and users pick from.",
    Dependencies = ["OrchardCore.Themes"],
    EnabledByDependencyOnly = true,
    Category = "Theming"
)]
