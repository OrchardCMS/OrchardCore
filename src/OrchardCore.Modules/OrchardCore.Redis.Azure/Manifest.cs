using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Azure Redis Cache",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Distributed cache using Azure Redis.",
    Dependencies =
    [
        "OrchardCore.Redis"
    ],
    Category = "Distributed"
)]
