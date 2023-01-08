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
    Description = "The roles module adds the permissions to assign roles to users. It's also provides a set of default roles for which other modules can define default permissions.",
    Dependencies = new[] { "OrchardCore.Roles.Updater" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Roles.Updater",
    Name = "Roles Updater",
    Description = "Updates the underlying default roles on each feature installation.",
    EnabledByDependencyOnly = true,
    Category = "Security"
)]
