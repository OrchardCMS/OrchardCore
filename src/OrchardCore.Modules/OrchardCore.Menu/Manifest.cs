using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Menu",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The Menu module provides menu management features.",
    Dependencies = new[]
    {
        "OrchardCore.Contents",
        "OrchardCore.Title",
        "OrchardCore.Alias",
        "OrchardCore.Recipes.Core",
    },
    Category = "Navigation"
)]
