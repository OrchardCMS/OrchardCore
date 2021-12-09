using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Roles",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The roles module adds the permissions to assign roles to users. It's also provides a set of default roles for which other modules can define default permissions.",
    Category = "Security"
)]
