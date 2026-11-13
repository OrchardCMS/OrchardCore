using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Dashboards",
    Name = "Dashboards",
    Description = "Provides dashboards made of widgets, that other features register types of, such as the Admin Dashboard.",
    Category = "Content Management",
    Dependencies =
    [
        "OrchardCore.Admin",
        "OrchardCore.Contents",
        "OrchardCore.Html",
        "OrchardCore.Title",
        "OrchardCore.Recipes.Core",
    ]
)]

[assembly: Feature(
    Id = "OrchardCore.Dashboards.User",
    Name = "My Dashboard",
    Description = "Gives every user a dashboard of their own, made of the widgets they choose.",
    Category = "Content Management",
    Dependencies =
    [
        "OrchardCore.Dashboards",
    ]
)]
