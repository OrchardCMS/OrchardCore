using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "HTTPS",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "This module will ensure HTTPS is used when accessing the website.",
    Category = "Security"
)]
