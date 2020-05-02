using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Preview",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The content Preview module enables live content edition and content preview.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
