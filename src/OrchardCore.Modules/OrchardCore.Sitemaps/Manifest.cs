using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Sitemaps",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Sitemaps",
    Name = "Sitemaps",
    Description = "Provides dynamic sitemap generation services.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
    },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Sitemaps.RazorPages",
    Name = "Sitemaps for Decoupled Razor Pages",
    Description = "Provides decoupled razor pages support for dynamic sitemap generation.",
    Dependencies = new[]
    {
        "OrchardCore.Sitemaps"
    },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Sitemaps.Cleanup",
    Name = "Sitemaps Cleanup",
    Description = "Cleanup sitemap cache files through a background task.",
    Dependencies = new[]
    {
        "OrchardCore.Sitemaps"
    },
    Category = "Content Management"
)]
