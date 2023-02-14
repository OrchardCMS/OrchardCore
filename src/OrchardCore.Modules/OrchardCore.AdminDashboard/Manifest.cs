using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Admin Dashboard",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Allows to organize widgets in an Admin Dashboard.",
    Dependencies = new[]
    {
        "OrchardCore.Admin",
        "OrchardCore.Html",
        "OrchardCore.Title",
        "OrchardCore.Recipes.Core",
    },
    Category = "Content Management"
)]
