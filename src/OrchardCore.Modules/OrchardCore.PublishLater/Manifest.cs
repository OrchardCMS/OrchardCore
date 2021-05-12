using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Publish Later",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The Publish Later module adds the ability to schedule content items to be published at a given future date and time.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
