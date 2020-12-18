using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Title",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The title module enables content items to have titles.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
