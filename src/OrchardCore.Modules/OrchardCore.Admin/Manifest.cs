using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Admin",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Admin",
    Name = "Admin",
     Description = "Creates an admin section for the site.",
    Category = "Infrastructure",
    Dependencies =
    [
        "OrchardCore.Settings"
    ]
)]

[assembly: Feature(
    Id = ManifestConstants.Features.BlazorUI,
    Name = "Blazor UI for Admin",
    Category = "Infrastructure",
    Description = "Adds support for the Blazor UI for Admin section.",
    Dependencies =
    [
        "OrchardCore.Admin"
    ]
)]
