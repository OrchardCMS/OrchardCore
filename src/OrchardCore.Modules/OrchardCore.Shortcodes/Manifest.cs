using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Shortcodes",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Shortcodes",
    Name = "Shortcodes",
    Description = "The Shortcodes feature adds shortcode capabilities.",
    Category = "Infrastructure"
)]

[assembly: Feature(
    Id = "OrchardCore.Shortcodes.Templates",
    Name = "Shortcode Templates",
    Description = "The Shortcode Templates feature provides a way to write custom shortcode templates from the admin.",
    Category = "Content",
    Dependencies = new[] { "OrchardCore.Liquid", "OrchardCore.Shortcodes" }
)]
