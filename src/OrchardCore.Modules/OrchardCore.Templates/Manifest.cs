using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Templates",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Templates",
    Name = "Templates",
    Description = "The Templates module provides a way to write custom shape templates from the admin.",
    Dependencies = new[] { "OrchardCore.Liquid" },
    Category = "Development"
)]

[assembly: Feature(
    Id = "OrchardCore.AdminTemplates",
    Name = "Admin Templates",
    Description = "The Admin Templates module provides a way to write custom admin shape templates.",
    Dependencies = new[] { "OrchardCore.Templates" },
    Category = "Development"
)]
