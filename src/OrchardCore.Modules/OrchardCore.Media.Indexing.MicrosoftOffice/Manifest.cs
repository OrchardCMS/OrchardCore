using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Microsoft Office Media Indexing",
    Description = "Provides a way to index Microsoft Office files including Word and Power Point files in search providers",
    Dependencies =
    [
        "OrchardCore.Media.Indexing"
    ],
    Category = "Search",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]
