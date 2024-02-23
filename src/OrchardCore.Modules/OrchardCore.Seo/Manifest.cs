using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "SEO",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides SEO meta features",
    Category = "Content Management",
    Dependencies =
    [
        "OrchardCore.Contents",
        "OrchardCore.Recipes.Core",
        "OrchardCore.Media",
    ]
)]
