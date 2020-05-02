using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Types",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Content Types modules enables the creation and alteration of content types not based on code.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
