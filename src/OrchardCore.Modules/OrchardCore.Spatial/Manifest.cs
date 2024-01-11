using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Spatial",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "This feature provides the ability to provide spatial locations to content items.",
    Dependencies = new[] { "OrchardCore.ContentTypes", "OrchardCore.Search.Lucene" },
    Category = "Content Management"
)]
