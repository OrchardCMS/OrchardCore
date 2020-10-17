using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Core Upgrade",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Upgrade.UserId",
    Name = "Upgrade UserId and Content Item OwnerId",
    Description = "Provides an upgrade path from RC2 to V1 for the UserId and OwnerId property.",
    Dependencies = new[] { "OrchardCore.Users" },
    Category = "Infrastructure"
)]
