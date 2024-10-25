using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "PDF Media Indexing",
    Description = "Provides a way to index PDF files in search providers.",
    Dependencies =
    [
        "OrchardCore.Media.Indexing"
    ],
    Category = "Search",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]
