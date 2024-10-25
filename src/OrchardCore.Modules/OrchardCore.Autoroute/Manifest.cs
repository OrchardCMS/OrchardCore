using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Autoroute",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Dependencies = ["OrchardCore.ContentTypes"],
    Category = "Navigation"
)]
