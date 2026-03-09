using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenApi",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.OpenApi",
    Name = "OpenApi",
    Description = "Microsoft.AspnetCore.OpenApi module for Orchard Core.",
    Category = "Api"
)]
