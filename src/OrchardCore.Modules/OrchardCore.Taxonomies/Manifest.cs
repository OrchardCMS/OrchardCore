using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Taxonomies",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Taxonomies",
    Name = "Taxonomies",
    Description = "The taxonomies module provides a way to categorize content items.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]

[assembly: Feature(
    Id = "OrchardCore.Taxonomies.ContentsAdminList",
    Name = "Taxonomies Contents List Filters",
    Description = "Provides taxonomy filters in the contents list.",
    Dependencies = new[] { "OrchardCore.Taxonomies" },
    Category = "Content Management"
)]
