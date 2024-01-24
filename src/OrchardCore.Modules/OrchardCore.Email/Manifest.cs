using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Email",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides email settings configuration.",
    Dependencies = ["OrchardCore.Resources"],
    Category = "Messaging",
    EnabledByDependencyOnly = true
)]

