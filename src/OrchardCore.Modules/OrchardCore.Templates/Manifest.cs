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
    Dependencies = ["OrchardCore.Liquid"],
    Category = "Development"
)]

[assembly: Feature(
    Id = "OrchardCore.AdminTemplates",
    Name = "Admin Templates",
    Description = "The Admin Templates module provides a way to write custom admin shape templates.",
    Dependencies = ["OrchardCore.Templates"],
    Category = "Development"
)]

[assembly: Feature(
    Id = "OrchardCore.Templates.Assisted",
    Name = "Assisted Templates",
    Description = "Lists the templates of the themes and features to view and clone them, suggests alternates and inspects the shapes of content items, from the Templates and Admin Templates lists.",
    Dependencies = ["OrchardCore.Templates"],
    Category = "Development"
)]
