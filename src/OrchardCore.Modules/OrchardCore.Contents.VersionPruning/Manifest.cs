using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Content Version Pruning",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides a background task to prune old content item versions.",
    Dependencies = ["OrchardCore.Contents"],
    Category = "Content Management"
)]
