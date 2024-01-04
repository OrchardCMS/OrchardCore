using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Media Indexing",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Indexing",
    Name = "Media Indexing",
    Description = "Provides a way to index media files with common format in search providers.",
    Dependencies =
    [
        "OrchardCore.Media"
    ],
    Category = "Search",
    EnabledByDependencyOnly = true
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Indexing.MicrosoftOffice",
    Name = "Microsoft Office Media Indexing",
    Description = "Provides a way to index Microsoft Office files including Word and Power Point files in search providers",
    Dependencies =
    [
        "OrchardCore.Media.Indexing"
    ],
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Indexing.Pdf",
    Name = "PDF Media Indexing",
    Description = "Provides a way to index PDF files in search providers.",
    Dependencies =
    [
        "OrchardCore.Media.Indexing"
    ],
    Category = "Search"
)]

[assembly: Feature(
    Id = "OrchardCore.Media.Indexing.Text",
    Name = "Text Media Indexing",
    Description = "Provides a way to index common text files like (.txt and .md) in search providers.",
    Dependencies =
    [
        "OrchardCore.Media.Indexing"
    ],
    Category = "Search"
)]
