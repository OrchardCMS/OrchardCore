using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Taxonomies",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The taxonomies module provides a way to categorize content items.",
    Dependencies = new[] { "OrchardCore.ContentTypes" },
    Category = "Content Management"
)]
