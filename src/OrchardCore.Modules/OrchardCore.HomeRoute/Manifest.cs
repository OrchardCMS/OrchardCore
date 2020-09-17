using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Home Route",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.HomeRoute",
    Name = "Home Route",
    Description = "Provides a way to set the route corresponding to the homepage of the site",
    Category = "Infrastructure"
)]
