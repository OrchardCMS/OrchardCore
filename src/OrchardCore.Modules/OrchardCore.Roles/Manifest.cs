using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Roles",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Roles",
    Name = "Roles",
    Description = "Provides permissions to assign roles to users. Additionally, it updates default roles with default permissions provided by features.",
    Dependencies = new[] { "OrchardCore.Roles.Core" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Roles.Core",
    Name = "Roles Core Services",
    Description = "Provides role core services.",
    EnabledByDependencyOnly = true,
    Category = "Security"
)]
