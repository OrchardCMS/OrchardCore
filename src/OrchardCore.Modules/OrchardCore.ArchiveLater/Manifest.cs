using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Archive Later",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The Archive Later module adds the ability to schedule content items to be archived at a given future date and time.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
