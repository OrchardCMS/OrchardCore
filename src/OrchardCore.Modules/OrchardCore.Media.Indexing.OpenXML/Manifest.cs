using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenXML Media Indexing",
    Description = "Provides a way to index Office files such as Word and Power Point in search providers",
    Dependencies =
    [
        "OrchardCore.Media.Indexing"
    ],
    Category = "Search",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]
