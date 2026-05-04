using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Autoroute",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides a way to automatically generate routes for content items based on their content type and title.",
    Dependencies =
    [
        "OrchardCore.ContentTypes",
        "OrchardCore.HomeRoute",
    ],
    Category = "Navigation"
)]
